using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Mixture
{
    public class MixtureRemapModeDrawer : MixturePropertyDrawer
    {
        protected override void DrawerGUI (Rect position, MaterialProperty prop, string label, MaterialEditor editor, MixtureGraph graph, MixtureNodeView nodeView)
        {
            //Enum(Brightness (Gradient),0,Alpha (Curve),1,Brightness (Curve),2,Saturation (Curve),3,Hue (Curve),4,Red (Curve),5,Green (Curve),6,Blue (Curve),7)

            int value = EditorGUI.IntPopup(position, label, (int)prop.floatValue, displayedOptions, optionValues);

            if (GUI.changed)
                prop.floatValue = (float)value;
        }

        static string[] displayedOptions = { "Brightness (Gradient)", "Red Channel (Curve)", "Green Channel (Curve)", "Blue Channel (Curve)", "Alpha Channel (Curve)", "All Channels (4 Curves)", "Brightness (Curve)", "Saturation (Curve)", "Hue (Curve)" };
        static int[] optionValues = { 0, 5, 6, 7, 1, 8, 2, 3, 4 };
    }
}