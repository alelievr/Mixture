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
	[NodeCustomEditor(typeof(ParticlesNode))]
	public class ParticlesNodeView : BasePrefabNodeView
	{
		ParticlesNode		particlesNode => nodeTarget as ParticlesNode;

        GameObject  openedPrefabRoot;
        string      openedPrefabPath;

        protected override bool showOpenPrefabButton => false;

		public override void Enable(bool fromInspector)
		{
			base.Enable(fromInspector);

            if (fromInspector)
            {
                var system = particlesNode.prefab.GetComponent<ParticleSystem>();
                var editor = Editor.CreateEditor(system);
                editor.hideFlags = HideFlags.HideAndDontSave;

                controlsContainer.Add(new IMGUIContainer(() => {
                    EditorGUI.BeginChangeCheck();
                    editor.OnInspectorGUI();
                    if (EditorGUI.EndChangeCheck())
                    {
                        PrefabUtility.SavePrefabAsset(particlesNode.prefab);
                        particlesNode.PlayParticleSystem();
                    }
                }));

                RegisterCallback<DetachFromPanelEvent>(e => {
                    if (editor != null)
                        UnityEngine.Object.DestroyImmediate(editor);
                });
            }

            // TODO: play, stop, pause, reset buttons for particle system
		}

        ~ParticlesNodeView()
        {
        }

        protected override bool OnPrefabOpened(PrefabStage stage, Button openPrefabButton)
        {
            if (!base.OnPrefabOpened(stage, openPrefabButton))
                return false;

            return true;
        }

        protected override bool OnPrefabClosed(PrefabStage stage, Button openPrefabButton)
        {
            if (!base.OnPrefabClosed(stage, openPrefabButton))
                return false;

            owner.graph.NotifyNodeChanged(nodeTarget);

            return true;
        }

        protected override void OpenPrefab()
        {
            base.OpenPrefab();
            owner.graph.NotifyNodeChanged(nodeTarget);
        }

		public override void OnRemoved()
        {
        }    
	}
}