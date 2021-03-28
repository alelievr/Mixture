using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Mixture
{
    [CustomEditor(typeof(MixtureGraphBehaviour))]
    public class MixtureGraphBehaviourEditor : Editor
    {
        Editor graphEditor;
        MixtureGraphBehaviour behaviour => target as MixtureGraphBehaviour;

        void OnEnable()
        {
            graphEditor = Editor.CreateEditor(behaviour.graph);
            Debug.Log(graphEditor);
        }

        void OnDisable()
        {
            DestroyImmediate(graphEditor);
        }

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();
            var graphContainer = graphEditor != null ? graphEditor.CreateInspectorGUI()?.Q("ExposedParameters") : null;

            root.Add(new Button(() => EditorWindow.GetWindow<MixtureGraphWindow>().InitializeGraph(behaviour.graph))
            {
                text = "Open"
            });

            root.Add(graphContainer);

            return root;
        }
    }
}