using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using GraphProcessor;
using UnityEditor.Experimental.SceneManagement;
using UnityEditor.SceneManagement;

namespace Mixture
{
	public class BasePrefabNodeView : MixtureNodeView
	{
		BasePrefabNode		sceneNode => nodeTarget as BasePrefabNode;

        GameObject  openedPrefabRoot;
        string      openedPrefabPath;

        protected virtual bool showOpenPrefabButton => true;

		public override void Enable(bool fromInspector)
		{
			base.Enable(fromInspector);

            var openPrefabButton = new Button(OpenPrefab) { text = "Open Prefab"};
            if (showOpenPrefabButton)
                controlsContainer.Add(openPrefabButton);

            if (!fromInspector)
            {
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
		}

        protected virtual bool OnPrefabOpened(PrefabStage stage, Button openPrefabButton)
        {
            if (stage.assetPath != AssetDatabase.GetAssetPath(sceneNode.prefab))
                return false;

            openPrefabButton.text = "Close Prefab";
            sceneNode.prefabOpened = true;
            openedPrefabRoot = stage.prefabContentsRoot;
            openedPrefabPath = stage.assetPath;

            return true;
        }

        protected virtual bool OnPrefabClosed(PrefabStage stage, Button openPrefabButton)
        {
            if (stage.assetPath != AssetDatabase.GetAssetPath(sceneNode.prefab))
                return false;

            openPrefabButton.text = "Open Prefab";
            sceneNode.prefabOpened = false;
            
            return true;
        }

        protected virtual void OpenPrefab()
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
        }
	}
}