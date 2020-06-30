using UnityEngine;
using UnityEditor;
using System;

namespace Mixture
{
    public class TooltipDrawerDecorator : MixturePropertyDrawer
    {
        public string tooltip;

        // Draw the property inside the given rect
        protected override void DrawerGUI (Rect position, MaterialProperty prop, string label, MaterialEditor editor, MixtureGraph graph, MixtureNodeView nodeView)
        {
            tooltip = label;
        }
    }
}