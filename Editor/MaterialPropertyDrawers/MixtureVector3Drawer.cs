using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MixtureVector3Drawer : MaterialPropertyDrawer
{
    public override void OnGUI (Rect position, MaterialProperty prop, string label, MaterialEditor editor)
    {
        EditorGUIUtility.wideMode = true;
        Vector3 value = EditorGUI.Vector3Field(position, prop.displayName, prop.vectorValue);

        if (GUI.changed)
            prop.vectorValue = new Vector4(value.x, value.y, value.z, 0);
    }
}
