using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using GraphProcessor;
using UnityEditor.Experimental.SceneManagement;

namespace Mixture
{
	[NodeCustomEditor(typeof(SceneNode))]
	public class SceneNodeView : MixtureNodeView
	{
		SceneNode		sceneNode => nodeTarget as SceneNode;

		public override void Enable()
		{
			base.Enable();

            controlsContainer.Add(new Button(OpenPrefab) { text = "Open Scene"});

            // TODO: dynamically add/remove this button if the scene is opened
            controlsContainer.Add(new Button(SaveView) { text = "Save Current View"});

            EditorApplication.update -= RenderPrefabScene;
            EditorApplication.update += RenderPrefabScene;

            // TODO: prefab field to put a custom prefab

            PrefabStage.prefabStageOpened -= PrefabOpened;
            PrefabStage.prefabStageOpened += PrefabOpened;
            PrefabStage.prefabStageClosing -= PrefabClosed;
            PrefabStage.prefabStageClosing += PrefabClosed;

            var stage = PrefabStageUtility.GetCurrentPrefabStage();
            if (stage != null && stage.assetPath == AssetDatabase.GetAssetPath(sceneNode.prefab))
                PrefabOpened(stage);
		}

        ~SceneNodeView()
        {
            EditorApplication.update -= RenderPrefabScene;
            PrefabClosed(null);
        }

        void RenderPrefabScene()
        {
            var stage = PrefabStageUtility.GetCurrentPrefabStage();
            if (sceneNode.prefabCamera != null && stage != null)
            {
                sceneNode.prefabCamera.scene = stage.scene;
                sceneNode.Render(sceneNode.prefabCamera);
            }
        }

        void PrefabOpened(PrefabStage stage)
        {
            if (stage.assetPath != AssetDatabase.GetAssetPath(sceneNode.prefab))
                return;

            // Prefabs can only have one root GO (i guess?)
            var root = stage.scene.GetRootGameObjects()[0];
            
            sceneNode.prefabOpened = true;
            sceneNode.prefabCamera = root.GetComponentInChildren<Camera>();
            sceneNode.bufferOutput = root.GetComponentInChildren<MixtureBufferOutput>();
            if (sceneNode.prefabCamera == null)
                Debug.LogError("No camera found in prefab, Please add one and re-open the prefab");
        }

        void PrefabClosed(PrefabStage stage)
        {
            if (stage.assetPath != AssetDatabase.GetAssetPath(sceneNode.prefab))
                return;

            sceneNode.prefabOpened = false;
            sceneNode.prefabCamera = null;
            sceneNode.bufferOutput = null;
        }

        void OpenPrefab()
        {
            var path = AssetDatabase.GetAssetPath(sceneNode.prefab);
            AssetDatabase.OpenAsset(sceneNode.prefab);
        }

        void SaveView() => sceneNode.SaveCurrentViewToImage();

		public override void OnRemoved()
        {
            owner.graph.RemoveObjectFromGraph(sceneNode.output);
        }    
	}
}