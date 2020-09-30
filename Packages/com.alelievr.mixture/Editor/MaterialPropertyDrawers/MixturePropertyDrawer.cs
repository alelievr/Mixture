using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using UnityEngine.Rendering;

namespace Mixture
{
    public class MixturePropertyDrawer : MaterialPropertyDrawer
    {
        class MixtureDrawerInfo
        {
            public MixtureGraph     graph;
            public MixtureNodeView  nodeView;
        }

        static Dictionary<MaterialEditor, MixtureDrawerInfo>    mixtureDrawerInfos = new Dictionary<MaterialEditor, MixtureDrawerInfo>();

        public static void RegisterEditor(MaterialEditor editor, MixtureNodeView nodeView, MixtureGraph graph)
        {
            mixtureDrawerInfos[editor] = new MixtureDrawerInfo{ graph = graph, nodeView = nodeView };
        }

        public static void UnregisterGraph(MixtureGraph graph)
        {
            foreach (var kp in mixtureDrawerInfos)
            {
                if (kp.Value.graph == graph)
                    mixtureDrawerInfos.Remove(kp.Key);
            }
        }

        protected MixtureGraph GetGraph(MaterialEditor editor) => mixtureDrawerInfos[editor].graph;
        protected MixtureNodeView GetNodeView(MaterialEditor editor) => mixtureDrawerInfos[editor].nodeView;

        public sealed override void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
        {
            // In case the material is shown in the inspector, the editor will not be linked to a node
            if (!mixtureDrawerInfos.ContainsKey(editor))
            {
                DrawerGUI(position, prop, label, editor, null, null);
                return;
            }

            var nodeView = GetNodeView(editor);
            var graph = GetGraph(editor);

            DrawerGUI(position, prop, label, editor, graph, nodeView);
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            if (!mixtureDrawerInfos.ContainsKey(editor))
                return base.GetPropertyHeight(prop, label, editor);

            return base.GetPropertyHeight(prop, label, editor);
        }

        protected virtual void DrawerGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor, MixtureGraph graph, MixtureNodeView nodeView) {}
    }
}