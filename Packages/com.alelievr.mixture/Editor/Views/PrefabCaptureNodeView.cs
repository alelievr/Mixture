using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using GraphProcessor;
#if UNITY_2021_2_OR_NEWER
using UnityEditor.SceneManagement;
#else
using UnityEditor.Experimental.SceneManagement;
#endif

namespace Mixture
{
	[NodeCustomEditor(typeof(PrefabCaptureNode))]
	public class PrefabCaptureNodeView : BasePrefabNodeView
	{
		PrefabCaptureNode		sceneNode => nodeTarget as PrefabCaptureNode;

        GameObject  openedPrefabRoot;
        string      openedPrefabPath;


		public override void Enable(bool fromInspector)
		{
			base.Enable(fromInspector);

            controlsContainer.Add(new Button(SaveView) { text = "Save Current View"});

            if (!fromInspector)
            {
                EditorApplication.update -= RenderPrefabScene;
                EditorApplication.update += RenderPrefabScene;

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

        protected override bool OnPrefabOpened(PrefabStage stage, Button openPrefabButton)
        {
            if (!base.OnPrefabOpened(stage, openPrefabButton))
                return false;

            // Prefabs can only have one root GO (i guess?)
            var root = stage.scene.GetRootGameObjects()[0];
            
            sceneNode.prefabCamera = root.GetComponentInChildren<Camera>();
            sceneNode.bufferOutput = root.GetComponentInChildren<MixtureBufferOutput>();
            if (sceneNode.prefabCamera == null)
                Debug.LogError("No camera found in prefab, Please add one and re-open the prefab");

            return true;
        }

        protected override bool OnPrefabClosed(PrefabStage stage, Button openPrefabButton)
        {
            if (!base.OnPrefabClosed(stage, openPrefabButton))
                return false;

            sceneNode.prefabCamera = null;
            sceneNode.bufferOutput = null;

            owner.graph.NotifyNodeChanged(nodeTarget);

            return true;
        }

        protected override void OpenPrefab()
        {
            base.OpenPrefab();
            owner.graph.NotifyNodeChanged(nodeTarget);
        }

        void SaveView() => sceneNode.SaveCurrentViewToImage();

		public override void OnRemoved()
        {
            owner.graph.RemoveObjectFromGraph(sceneNode.savedTexture);
        }    
	}
}