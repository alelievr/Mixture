using UnityEngine;
using GraphProcessor;
using UnityEngine.Experimental.Rendering;
using System.IO;
using UnityEngine.Rendering;
using System.Diagnostics;
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

Opening the prefab will switch to a render texture so you can visualize the changes in real-time in the graph.
When you are satisfied with the setup in the prefab, click on 'Save Current View' to save the texture as sub-asset of the graph, you cna the close the prefab and the scene node will use this baked texture as output.

Note that this node is currently only available with HDRP.
")]

	[System.Serializable, NodeMenuItem("Utils/Scene Capture (HDRP only)")]
	public class SceneNode : MixtureNode
	{
        [System.Serializable]
        public enum OutputMode
        {
            // Keep indices to not break shaders that depend on it
            Color           = 0,
            LinearEyeDepth  = 1,
            Linear01Depth   = 2,
            WorldNormal     = 3,
            TangentNormal   = 4,
            WorldPosition   = 5,
        }

        [SerializeField, HideInInspector, FormerlySerializedAs("output")]
		internal Texture2D savedTexture;

        [Tooltip("Rendered view from the camera in the prefab")]
		[Output(name = "Output")]
        [System.NonSerialized]
        public Texture outputTexture;

        public OutputMode mode;

		public override bool 	hasSettings => false;
		public override string	name => "Scene Capture (HDRP only)";
		public override float	nodeWidth => 200;
		public override Texture	previewTexture => prefabOpened ? (Texture)tmpRenderTexture : savedTexture;

        public override bool    showDefaultInspector => true;
        public override bool    showPreviewExposure => mode == OutputMode.LinearEyeDepth;

        public GameObject       prefab;

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

        protected override void Disable()
        {
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
            // Radback color & depth:
            RenderTexture.active = tmpRenderTexture;
            savedTexture.ReadPixels(new Rect(0, 0, savedTexture.width, savedTexture.height), 0, 0);
            RenderTexture.active = null; 
            savedTexture.Apply();

            graph.NotifyNodeChanged(this);

            if (!graph.IsObjectInGraph(savedTexture))
                graph.AddObjectToGraph(savedTexture);
        }

        void UpdateRenderTextures()
        {
            UpdateTempRenderTexture(ref tmpRenderTexture);
            if (savedTexture == null || rtSettings.NeedsUpdate(graph, savedTexture))
            {
                if (graph.IsObjectInGraph(savedTexture))
                    graph.RemoveObjectFromGraph(savedTexture);
                savedTexture = new Texture2D(rtSettings.GetWidth(graph), rtSettings.GetHeight(graph), rtSettings.GetGraphicsFormat(graph), TextureCreationFlags.None) { name = "SceneNode Rendering"};
            }
        }
	}
}