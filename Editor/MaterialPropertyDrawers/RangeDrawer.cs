using UnityEngine;
using UnityEditor;

namespace Mixture
{
    public class RangeDrawer : MixturePropertyDrawer
    {
        protected override void DrawerGUI (Rect position, MaterialProperty prop, string label, MaterialEditor editor, MixtureGraph graph, MixtureNodeView nodeView)
        {
            prop.floatValue = EditorGUI.Slider(position, prop.displayName, prop.floatValue, prop.rangeLimits.x, prop.rangeLimits.y);
        }
    }
}