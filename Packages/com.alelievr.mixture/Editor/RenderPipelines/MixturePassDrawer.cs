using UnityEngine;
using UnityEditor.Rendering.HighDefinition;
using UnityEditor;
using Mixture;
using GraphProcessor;

[CustomPassDrawerAttribute(typeof(MixturePass))]
class MixturePassDrawer : CustomPassDrawer
{
    static float lh = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
    int paramCount = 0;

    protected override void DoPassGUI(SerializedProperty customPass, Rect rect)
    {
        
        var graphRect = rect;
        graphRect.xMax -= 90;
        var buttonRect = rect;
        buttonRect.xMin = graphRect.xMax;
        EditorGUI.PropertyField(graphRect, customPass.FindPropertyRelative("graph"));
        var graph = (target as MixturePass).graphReference;

        if (GUI.Button(buttonRect, new GUIContent("Open Graph")))
        {
            if (graph != null)
                MixtureGraphWindow.Open(graph);
        }

        rect.y += lh;

        if (graph == null)
            return;
        
        paramCount = 0;
        foreach (var p in graph.exposedParameters)
        {
            if (p.settings.isHidden)
                continue;

            MixtureRenderPipelineEditorUtils.DrawExposedParameter(p, rect);
            
            rect.y += lh;
            paramCount++;
        }
    }

    protected override float GetPassHeight(SerializedProperty customPass)
    {
        return lh * (paramCount + 1);
    }
}