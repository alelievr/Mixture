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

        static string[] displayedOptions = { "Input.Red", "Input.Green", "Input.Blue", "Input.Alpha", "Black", "Gray", "White", "Custom" };
        static int[] optionValues = { 0, 1, 2, 3, 4, 5, 6, 7 };
    }
}