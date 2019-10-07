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
            public MixtureGraph    graph;
            public MixtureNodeView nodeView;
        }

        static Dictionary<MaterialEditor, MixtureDrawerInfo>    mixtureDrawerInfos = new Dictionary<MaterialEditor, MixtureDrawerInfo>();
        List<TextureDimension>                                  allowedDimensions = new List<TextureDimension>();
        
        public static void RegisterEditor(MaterialEditor editor, MixtureNodeView nodeView, MixtureGraph graph)
        {
            if (mixtureDrawerInfos.ContainsKey(editor))
            {
                mixtureDrawerInfos[editor].graph = graph;
                mixtureDrawerInfos[editor].nodeView = nodeView;
            }
            else
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
            var node = nodeView.nodeTarget as MixtureNode;

            allowedDimensions = MixtureUtils.GetAllowedDimentions(prop.name);

            if (IsVisible(editor))
                DrawerGUI(position, prop, label, editor, graph, nodeView);
        }

        bool IsVisible(MaterialEditor editor)
        {
            // Always display the drawer when inside the inspector (for debug)
            if (!mixtureDrawerInfos.ContainsKey(editor))
                return true;

            var nodeView = GetNodeView(editor);
            var graph = GetGraph(editor);
            var node = nodeView.nodeTarget as MixtureNode;

            // Draw only if the drawer support the current dimension of the node
            var currentDimension = node.rtSettings.GetTextureDimension(graph);
            return allowedDimensions.Any(dim => (int)dim == (int)currentDimension);
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            return IsVisible(editor) ? base.GetPropertyHeight(prop, label, editor) : -EditorGUIUtility.standardVerticalSpacing;
        }

        protected virtual void DrawerGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor, MixtureGraph graph, MixtureNodeView nodeView) {}
    }
}