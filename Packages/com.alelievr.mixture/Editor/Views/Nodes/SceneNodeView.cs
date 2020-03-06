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
        Camera          prefabCamera;

		protected override bool hasPreview => true;

		public override void Enable()
		{
			base.Enable();

            controlsContainer.Add(new Button(OpenPrefab) { text = "Open Scene"});
            controlsContainer.Add(new Button(SaveView) { text = "Save View Image"});

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
            if (prefabCamera != null && stage != null)
            {
                prefabCamera.scene = stage.scene;
                sceneNode.Render(prefabCamera);
            }
        }

        void PrefabOpened(PrefabStage stage)
        {
            sceneNode.prefabOpened = true;
            prefabCamera = sceneNode.prefab.GetComponentInChildren<Camera>();
            if (prefabCamera == null)
                Debug.LogError("No camera found in prefab, Please add one and re-open the prefab");
        }

        void PrefabClosed(PrefabStage stage)
        {
            sceneNode.prefabOpened = false;
            prefabCamera = null;
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