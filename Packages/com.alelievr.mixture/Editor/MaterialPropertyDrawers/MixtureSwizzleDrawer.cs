using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Mixture
{
    public class MixtureSwizzleDrawer : MixturePropertyDrawer
    {
        protected override void DrawerGUI (Rect position, MaterialProperty prop, string label, MaterialEditor editor, MixtureGraph graph, MixtureNodeView nodeView)
        {
            //[Enum(Red,0,Green,1,Blue,2,Alpha,3,Black,4,Gray,5,White,6,Custom,7)]
            
            int value = EditorGUI.IntPopup(position, label, (int)prop.floatValue, displayedOptions, optionValues);

            if (GUI.changed)
                prop.floatValue = (float)value;
        }

        static string[] displayedOptions = { 
            "Input A/Red", "Input A/Green", "Input A/Blue", "Input A/Alpha",
            "Input B/Red", "Input B/Green", "Input B/Blue", "Input B/Alpha",
            "Input C/Red", "Input C/Green", "Input C/Blue", "Input C/Alpha",
            "Input D/Red", "Input D/Green", "Input D/Blue", "Input D/Alpha",
            "Black", "Gray", "White", "Custom" };
        static int[] optionValues = { 0, 1, 2, 3,
            8, 9, 10, 11,
            12, 13, 14, 15,
            16, 17, 18, 19,
            4, 5, 6, 7 };
    }
}