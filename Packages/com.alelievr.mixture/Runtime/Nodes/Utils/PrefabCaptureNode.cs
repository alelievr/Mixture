using UnityEngine;
using GraphProcessor;
using UnityEngine.Experimental.Rendering;
using System.IO;
using UnityEngine.Rendering;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
#endif
using UnityEngine.Serialization;

namespace Mixture
{
    [Documentation(@"
Renders the content of the prefab using the camera at the root of the prefab.
You can use choose to output different buffers from the prefab: Color, Depth, World Normal, Tangent or World Position.
The alpha channel is used to know whether an object is here or not (0 means nothing and 1 object).

Opening the prefab will switch to a render texture so you can visualize the changes in real-time in the graph.
When you are satisfied with the setup in the prefab, click on 'Save Current View' to save the texture as sub-asset of the graph, you cna the close the prefab and the scene node will use this baked texture as output.

Note that this node is currently only available with HDRP.
")]

	[System.Serializable, NodeMenuItem("Utils/Prefab Capture (HDRP only)")]
	public class PrefabCaptureNode : MixtureNode, ICreateNodeFrom<GameObject>
	{
        [System.Serializable]
        public enum OutputMode
        {
            // Keep indices to not break shaders that depend on it
            Color               = 0,
            LinearEyeDepth      = 1,
            Linear01Depth       = 2,
            WorldSpaceNormal    = 3,
            TangentSpaceNormal  = 4,
            WorldPosition       = 5,
        }

        [SerializeField, HideInInspector, FormerlySerializedAs("output")]
		internal Texture2D savedTexture;

        [Tooltip("Rendered view from the camera in the prefab")]
		[Output]
        [System.NonSerialized]
        public Texture outputTexture;

        public OutputMode mode;

		public override bool 	hasSettings => true;
		public override string	name => "Prefab Capture (HDRP)";
		public override float	nodeWidth => 200;
		public override Texture	previewTexture => prefabOpened ? (Texture)tmpRenderTexture : savedTexture;

        public override bool    showDefaultInspector => true;
        public override bool    showPreviewExposure => mode == OutputMode.LinearEyeDepth;

		protected override MixtureRTSettings defaultRTSettings
        {
            get
            {
                var settings = base.defaultRTSettings;
                settings.editFlags = EditFlags.All ^ EditFlags.POTSize;
                return Get2DOnlyRTSettings(settings);
            }
        }

        public GameObject       prefab;

        [ShowInInspector]
        public TextureFormat    compressionFormat = TextureFormat.DXT5; 

        [System.NonSerialized]
        internal bool           prefabOpened = false;
#if UNITY_EDITOR
        [System.NonSerialized]
        bool                    createNewPrefab = false;
#endif
        [System.NonSerialized]
        internal Camera         prefabCamera;
        internal MixtureBufferOutput bufferOutput;

        // We don't use the 'Custom' part of the render texture but function are taking this type in parameter
        internal CustomRenderTexture     tmpRenderTexture;

        public override void OnNodeCreated()
        {
            base.OnNodeCreated();

#if UNITY_EDITOR
            createNewPrefab = true;
#endif
        }

		public bool InitializeNodeFromObject(GameObject value)
		{
            createNewPrefab = false;
			prefab = value;
			return true;
		}

#if UNITY_EDITOR
        GameObject LoadDefaultPrefab()
        {
            return Resources.Load<GameObject>("Scene Capture Node Prefab");
        }

        GameObject SavePrefab(GameObject sceneObject)
        {
            string dirPath = Path.GetDirectoryName(graph.mainAssetPath) + "/" + graph.name;
            if (!Directory.Exists(dirPath))
                Directory.CreateDirectory(dirPath);

            string prefabPath = AssetDatabase.GenerateUniqueAssetPath(dirPath + "/" + "SceneCapture.prefab");

            return PrefabUtility.SaveAsPrefabAssetAndConnect(sceneObject, prefabPath, InteractionMode.UserAction);
        }
#endif

        protected override void Enable()
        {
#if UNITY_EDITOR
            if (createNewPrefab)
            {
                // Create and save the new prefab
                var defaultPrefab = GameObject.Instantiate(LoadDefaultPrefab());
                prefab = SavePrefab(defaultPrefab);
                MixtureUtils.DestroyGameObject(defaultPrefab);
                ProjectWindowUtil.ShowCreatedAsset(prefab);
                EditorGUIUtility.PingObject(prefab);
            }
#endif
            UpdateRenderTextures();
        }

        [CustomPortBehavior(nameof(outputTexture))]
		IEnumerable< PortData > OutputTextureType(List< SerializableEdge > edges)
        {
            yield return new PortData
            {
                displayName = "Output",
                displayType = typeof(Texture2D),
                acceptMultipleEdges = true,
                identifier = "Output"
            };
        }


        protected override void Disable()
        {
            base.Disable();

            if (prefabCamera != null)
                prefabCamera.targetTexture = null;

            CoreUtils.Destroy(tmpRenderTexture);
        }

        protected override bool ProcessNode(CommandBuffer cmd)
        {
            if (!base.ProcessNode(cmd))
                return false;

            UpdateRenderTextures();

            if (prefabOpened && tmpRenderTexture.updateCount > 1)
                outputTexture = tmpRenderTexture;
            else
                outputTexture = savedTexture;

            return true;
        }

        internal void Render(Camera cam)
        {
            cam.targetTexture = tmpRenderTexture;

            if (bufferOutput != null)
                bufferOutput.SetOutputSettings(mode, cam);

            cam.rect = new Rect(0, 0, 1, 1);
            cam.Render();
        }

        internal void SaveCurrentViewToImage()
        {
            // Temp texture for the readback (before compression)
            Texture2D tmp = new Texture2D(savedTexture.width, savedTexture.height, GraphicsFormat.R32G32B32A32_SFloat, TextureCreationFlags.None);
            // Radback color & depth:
            RenderTexture.active = tmpRenderTexture;
            tmp.ReadPixels(new Rect(0, 0, savedTexture.width, savedTexture.height), 0, 0);
            RenderTexture.active = null; 
            tmp.Apply();

#if UNITY_EDITOR
            if (GraphicsFormatUtility.IsCompressedFormat(savedTexture.graphicsFormat))
            {
                EditorUtility.CopySerialized(tmp, savedTexture);
                Object.DestroyImmediate(tmp);
                EditorUtility.CompressTexture(savedTexture, compressionFormat, TextureCompressionQuality.Best);
            }
            else
            {
                savedTexture.SetPixels(tmp.GetPixels());
                savedTexture.Apply();
            }
#endif

            graph.NotifyNodeChanged(this);

            if (!graph.IsObjectInGraph(savedTexture))
                graph.AddObjectToGraph(savedTexture);
        }

        void UpdateRenderTextures()
        {
            UpdateTempRenderTexture(ref tmpRenderTexture);
            var compressedFormat = GraphicsFormatUtility.GetGraphicsFormat(compressionFormat, false);

            if (savedTexture == null || rtSettings.NeedsUpdate(graph, savedTexture, false))
            {
                if (graph.IsObjectInGraph(savedTexture))
                {
                    graph.RemoveObjectFromGraph(savedTexture);
                    Object.DestroyImmediate(savedTexture, true);
                }
                savedTexture = new Texture2D(rtSettings.GetWidth(graph), rtSettings.GetHeight(graph), compressedFormat, TextureCreationFlags.None) { name = "SceneNode Rendering"};
                savedTexture.hideFlags = HideFlags.NotEditable;
                graph.AddObjectToGraph(savedTexture);
            }

#if UNITY_EDITOR
            // Change texture format without touching the asset:
            if (savedTexture.graphicsFormat != compressedFormat)
            {
                if (GraphicsFormatUtility.IsCompressedFormat(compressedFormat))
                    EditorUtility.CompressTexture(savedTexture, compressionFormat, TextureCompressionQuality.Best);
                else
                {
                    var pixels = savedTexture.GetPixels();
                    var tmp = new Texture2D(savedTexture.width, savedTexture.height, compressedFormat, TextureCreationFlags.None);
                    tmp.SetPixels(pixels);
                    EditorUtility.CopySerialized(tmp, savedTexture);
                    Object.DestroyImmediate(tmp);
                }
            }
#endif
        }
	}
}