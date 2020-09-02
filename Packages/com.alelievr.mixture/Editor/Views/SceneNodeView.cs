using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using GraphProcessor;
using UnityEditor.Experimental.SceneManagement;
using UnityEditor.SceneManagement;

namespace Mixture
{
	[NodeCustomEditor(typeof(SceneNode))]
	public class SceneNodeView : MixtureNodeView
	{
		SceneNode		sceneNode => nodeTarget as SceneNode;

        GameObject  openedPrefabRoot;
        string      openedPrefabPath;

		public override void Enable(bool fromInspector)
		{
			base.Enable(fromInspector);

            var openPrefabButton = new Button(OpenPrefab) { text = "Open Scene"};
            controlsContainer.Add(openPrefabButton);

            // TODO: dynamically add/remove this button if the scene is opened
            controlsContainer.Add(new Button(SaveView) { text = "Save Current View"});

            EditorApplication.update -= RenderPrefabScene;
            EditorApplication.update += RenderPrefabScene;

            // TODO: prefab field to put a custom prefab

            PrefabStage.prefabStageOpened -= PrefabOpened;
            PrefabStage.prefabStageOpened += PrefabOpened;
            PrefabStage.prefabStageClosing -= PrefabClosed;
            PrefabStage.prefabStageClosing += PrefabClosed;

            void PrefabOpened(PrefabStage stage) => OnPrefabOpened(stage, openPrefabButton);
            void PrefabClosed(PrefabStage stage) => OnPrefabClosed(stage, openPrefabButton);

            var stage = PrefabStageUtility.GetCurrentPrefabStage();
            if (stage != null && stage.assetPath == AssetDatabase.GetAssetPath(sceneNode.prefab))
                PrefabOpened(stage);
		}

        ~SceneNodeView()
        {
            EditorApplication.update -= RenderPrefabScene;
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

        void OnPrefabOpened(PrefabStage stage, Button openPrefabButton)
        {
            if (stage.assetPath != AssetDatabase.GetAssetPath(sceneNode.prefab))
                return;

            // Prefabs can only have one root GO (i guess?)
            var root = stage.scene.GetRootGameObjects()[0];
            
            openPrefabButton.text = "Close Prefab";
            sceneNode.prefabOpened = true;
            sceneNode.prefabCamera = root.GetComponentInChildren<Camera>();
            sceneNode.bufferOutput = root.GetComponentInChildren<MixtureBufferOutput>();
            if (sceneNode.prefabCamera == null)
                Debug.LogError("No camera found in prefab, Please add one and re-open the prefab");

            openedPrefabRoot = stage.prefabContentsRoot;
            openedPrefabPath = stage.assetPath;
        }

        void OnPrefabClosed(PrefabStage stage, Button openPrefabButton)
        {
            if (stage.assetPath != AssetDatabase.GetAssetPath(sceneNode.prefab))
                return;

            openPrefabButton.text = "Open Prefab";
            sceneNode.prefabOpened = false;
            sceneNode.prefabCamera = null;
            sceneNode.bufferOutput = null;
        }

        void OpenPrefab()
        {
            if (!sceneNode.prefabOpened)
            {
                var path = AssetDatabase.GetAssetPath(sceneNode.prefab);
                AssetDatabase.OpenAsset(sceneNode.prefab);
            }
            else
            {
                PrefabUtility.SaveAsPrefabAsset(openedPrefabRoot, openedPrefabPath);
                StageUtility.GoBackToPreviousStage();
            }
        }

        void SaveView() => sceneNode.SaveCurrentViewToImage();

		public override void OnRemoved()
        {
            owner.graph.RemoveObjectFromGraph(sceneNode.savedTexture);
        }    
	}
}