using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MixtureVector2Drawer : MaterialPropertyDrawer
{
    public override void OnGUI (Rect position, MaterialProperty prop, string label, MaterialEditor editor)
    {
        EditorGUIUtility.wideMode = true;
           Vector2 value = EditorGUI.Vector2Field(position, prop.displayName, prop.vectorValue);

        if (GUI.changed)
            prop.vectorValue = new Vector4(value.x, value.y, 0, 0);
    }
}
