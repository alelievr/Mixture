using UnityEngine;
using UnityEditor;

namespace Mixture
{
    public class MixtureBlendDrawer : MixturePropertyDrawer
    {
        public enum Blend
        {
            [InspectorName("Normal (Copy)")]
            Normal = 0,
            [InspectorName("Min (Darken)")]
            Min = 1,
            [InspectorName("Max (Lighten)")]
            Max = 2,
            [InspectorName("Additive (Linear Dodge)")]
            Additive = 13,
            Subtract = 22,
            Burn = 3,
            Difference = 5,
            Dodge = 6,
            Divide = 7,
            Exclusion = 8,
            HardLight = 9,
            HardMix = 10,
            LinearBurn = 12,
            LinearLight = 14,
            LinearLightAddSub = 15,
            Multiply = 16,
            Negation = 17,
            Overlay = 18,
            PinLight = 19,
            Screen = 20,
            SoftLight = 21,
            VividLight = 23,
            Transparent = 24,
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