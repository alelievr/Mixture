using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Mixture
{
    public class ScaleBiasDrawer : MixturePropertyDrawer
    {
        protected override void DrawerGUI (Rect position, MaterialProperty prop, string label, MaterialEditor editor, MixtureGraph graph, MixtureNodeView nodeView)
        {
            //Enum(ScaleBias,0,BiasScale,1,Scale,2,Bias,3,TwiceMinusOne,4,HalvePlusHalf,5)

            int value = EditorGUI.IntPopup(position, label, (int)prop.floatValue, displayedOptions, optionValues);

            if (GUI.changed)
                prop.floatValue = (float)value;
        }

        static string[] displayedOptions = { "Scale Bias", "Bias Scale", "×2 -1 ", "×0.5 +0.5", "Scale", "Bias"};
        static int[] optionValues = { 0, 1, 4, 5, 2, 3};
    }
}