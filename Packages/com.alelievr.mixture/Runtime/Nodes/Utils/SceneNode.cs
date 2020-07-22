using UnityEngine;
using GraphProcessor;
using UnityEngine.Experimental.Rendering;
using System.IO;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
#endif

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Utils/Scene Capture")]
	public class SceneNode : MixtureNode
	{
        public enum OutputMode
        {
            Color,
            // Depth, // TODO
            // Normal,
        }

		[Output(name = "Output")]
		public Texture2D output;

        public OutputMode mode;

		public override bool 	hasSettings => false;
		public override string	name => "Scene Capture";
		public override float	nodeWidth => 200;
		public override Texture	previewTexture => prefabOpened ? (Texture)tmpRenderTexture : output;

        public override bool    showDefaultInspector => true;

        public GameObject       prefab;

        [System.NonSerialized]
        internal bool           prefabOpened = false;
        [System.NonSerialized]
        bool                    createNewPrefab = false;

        // We don't use the 'Custom' part of the render texture but function are taking this type in parameter
        CustomRenderTexture     tmpRenderTexture;

        public override void OnNodeCreated()
        {
            base.OnNodeCreated();

#if UNITY_EDITOR
            createNewPrefab = true;
#endif
        }

        GameObject CreateDefaultPrefab()
        {
            // TODO: make a default prefab object in the settings
            var prefab = new GameObject("Scene Capture Node Prefab");
            // TODO: enable this when it works
            // prefab.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;

            // Add a camera for rendering things:
            var cam = new GameObject("Camera", typeof(Camera));
            cam.transform.SetParent(prefab.transform, false);
            cam.transform.position = new Vector3(0, 0, -2);

            // And a cube
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.SetParent(prefab.transform);
            cube.transform.position = Vector3.zero;

            return prefab;
        }

        GameObject SavePrefab(GameObject sceneObject)
        {
#if UNITY_EDITOR
            Debug.Log(graph.mainAssetPath);
            string dirPath = Path.GetDirectoryName(graph.mainAssetPath) + "/" + graph.name;
            if (!Directory.Exists(dirPath))
                Directory.CreateDirectory(dirPath);

            string prefabPath = AssetDatabase.GenerateUniqueAssetPath(dirPath + "/" + "SceneCapture.prefab");

            return PrefabUtility.SaveAsPrefabAssetAndConnect(sceneObject, prefabPath, InteractionMode.UserAction);
#else
    return null;
#endif
        }

        protected override void Enable()
        {
            if (createNewPrefab)
            {
                // Create and save the new prefab
                var defaultPrefab = CreateDefaultPrefab();
                prefab = SavePrefab(defaultPrefab);
                MixtureUtils.DestroyGameObject(defaultPrefab);
#if UNITY_EDITOR
                ProjectWindowUtil.ShowCreatedAsset(prefab);
#endif
            }
            UpdateRenderTextures();
        }

        protected override bool ProcessNode()
        {
            UpdateRenderTextures();

            // get camera of the prefab, assign it render textures and readback

            return true;
        }

        internal void Render(Camera cam)
        {
            cam.targetTexture = tmpRenderTexture;
            cam.SetReplacementShader(Shader.Find("HDRP/Unlit"), "Opaque");
            cam.Render();
        }

        internal void SaveCurrentViewToImage()
        {
            // Radback color & depth:
            RenderTexture.active = tmpRenderTexture;
            output.ReadPixels(new Rect(0, 0, output.width, output.height), 0, 0);
            RenderTexture.active = null; 
            output.Apply();

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
                output = new Texture2D(rtSettings.width, rtSettings.height, rtSettings.GetGraphicsFormat(graph), TextureCreationFlags.None);
            }
        }
	}
}