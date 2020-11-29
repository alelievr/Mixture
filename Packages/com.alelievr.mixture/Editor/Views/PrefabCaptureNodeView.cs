using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using GraphProcessor;
using UnityEditor.Experimental.SceneManagement;
using UnityEditor.SceneManagement;

namespace Mixture
{
	[NodeCustomEditor(typeof(PrefabCaptureNode))]
	public class PrefabCaptureNodeView : MixtureNodeView
	{
		PrefabCaptureNode		sceneNode => nodeTarget as PrefabCaptureNode;

        GameObject  openedPrefabRoot;
        string      openedPrefabPath;


		public override void Enable(bool fromInspector)
		{
			base.Enable(fromInspector);

            var openPrefabButton = new Button(OpenPrefab) { text = "Open Prefab"};
            controlsContainer.Add(openPrefabButton);
            controlsContainer.Add(new Button(SaveView) { text = "Save Current View"});

            if (!fromInspector)
            {
                EditorApplication.update -= RenderPrefabScene;
                EditorApplication.update += RenderPrefabScene;

                PrefabStage.prefabStageOpened -= PrefabOpened;
                PrefabStage.prefabStageOpened += PrefabOpened;
                PrefabStage.prefabStageClosing -= PrefabClosed;
                PrefabStage.prefabStageClosing += PrefabClosed;

                void PrefabOpened(PrefabStage stage) => OnPrefabOpened(stage, openPrefabButton);
                void PrefabClosed(PrefabStage stage) => OnPrefabClosed(stage, openPrefabButton);

                var stage = PrefabStageUtility.GetCurrentPrefabStage();
                if (stage != null && stage.assetPath == AssetDatabase.GetAssetPath(sceneNode.prefab))
                    PrefabOpened(stage);
                
                ObjectField debugTextureField = new ObjectField("Saved Texture") { value = sceneNode.savedTexture };
                debugContainer.Add(debugTextureField);
                nodeTarget.onProcessed += () => debugTextureField.SetValueWithoutNotify(sceneNode.savedTexture);
            }
		}

        ~PrefabCaptureNodeView()
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

            owner.graph.NotifyNodeChanged(nodeTarget);
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
                if (openedPrefabRoot != null)
                    PrefabUtility.SaveAsPrefabAsset(openedPrefabRoot, openedPrefabPath);
                StageUtility.GoBackToPreviousStage();
            }

            owner.graph.NotifyNodeChanged(nodeTarget);
        }

        void SaveView() => sceneNode.SaveCurrentViewToImage();

		public override void OnRemoved()
        {
            owner.graph.RemoveObjectFromGraph(sceneNode.savedTexture);
        }    
	}
}