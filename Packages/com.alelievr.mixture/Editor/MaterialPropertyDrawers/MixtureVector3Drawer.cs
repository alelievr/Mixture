using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Mixture
{
    public class MixtureVector3Drawer : MixturePropertyDrawer
    {
        protected override void DrawerGUI (Rect position, MaterialProperty prop, string label, MaterialEditor editor, MixtureGraph graph, MixtureNodeView nodeView)
        {
            EditorGUIUtility.wideMode = true;
            Vector3 value = EditorGUI.Vector3Field(position, prop.displayName, prop.vectorValue);

            if (GUI.changed)
                prop.vectorValue = new Vector4(value.x, value.y, value.z, 0);
        }
    }
}