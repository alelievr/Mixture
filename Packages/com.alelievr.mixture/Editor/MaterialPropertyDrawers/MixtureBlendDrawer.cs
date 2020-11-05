using UnityEngine;
using UnityEditor;

namespace Mixture
{
    public class MixtureBlendDrawer : MixturePropertyDrawer
    {
        public enum Blend
        {
            Normal, Min, Max, Burn, Darken, Difference, Dodge, Divide, Exclusion, HardLight, HardMix,
            Lighten, LinearBurn, LinearDodge, LinearLight, LinearLightAddSub, Multiply,
            Negation, Overlay, PinLight, Screen, SoftLight, Subtract, VividLight
        }

        protected override void DrawerGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor, MixtureGraph graph, MixtureNodeView nodeView)
        {
            EditorGUI.BeginChangeCheck();
            int value = (int)(Blend)EditorGUI.EnumPopup(position, label, (Blend)(int)prop.floatValue);
            if (EditorGUI.EndChangeCheck())
                prop.floatValue = (float)value;
        }
    }
}