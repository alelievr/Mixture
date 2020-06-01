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

using Debug = UnityEngine.Debug;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Utils/Scene Capture")]
	public class SceneNode : MixtureNode
	{
        [System.Serializable]
        public enum OutputMode
        {
            Color,
            LinearEyeDepth,
            Linear01Depth,
            WorldNormal,
        }

		[Output(name = "Output")]
		public Texture2D output;

        public OutputMode mode;

		public override bool 	hasSettings => false;
		public override string	name => "Scene Capture";
		public override float	nodeWidth => 200;
		public override Texture	previewTexture => prefabOpened ? (Texture)tmpRenderTexture : output;

        public override bool    showDefaultInspector => true;
        public override bool    showPreviewExposure => mode == OutputMode.LinearEyeDepth;

        public GameObject       prefab;

        [System.NonSerialized]
        internal bool           prefabOpened = false;
        [System.NonSerialized]
        bool                    createNewPrefab = false;
        [System.NonSerialized]
        internal Camera         prefabCamera;
        internal MixtureBufferOutput bufferOutput;

        // We don't use the 'Custom' part of the render texture but function are taking this type in parameter
        CustomRenderTexture     tmpRenderTexture;

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
            UpdateRenderTextures();

            // get camera of the prefab, assign it render textures and readback

            return true;
        }

        internal void Render(Camera cam)
        {
            cam.targetTexture = tmpRenderTexture;

            if (bufferOutput != null)
                bufferOutput.SetOutputMode(mode);

            cam.rect = new Rect(0, 0, 1, 1);
            cam.Render();
        }

        internal void SaveCurrentViewToImage()
        {
            // Radback color & depth:
            RenderTexture.active = tmpRenderTexture;
            output.ReadPixels(new Rect(0, 0, output.width, output.height), 0, 0);
            RenderTexture.active = null; 
            output.Apply();
        
            graph.NotifyNodeChanged(this);

            if (!graph.IsObjectInGraph(output))
                graph.AddObjectToGraph(output);
        }

        void UpdateRenderTextures()
        {
            UpdateTempRenderTexture(ref tmpRenderTexture);
            if (output == null || rtSettings.NeedsUpdate(graph, output))
            {
                if (graph.IsObjectInGraph(output))
                    graph.RemoveObjectFromGraph(output);
                output = new Texture2D(rtSettings.GetWidth(graph), rtSettings.GetHeight(graph), rtSettings.GetGraphicsFormat(graph), TextureCreationFlags.None);
            }
        }
	}
}