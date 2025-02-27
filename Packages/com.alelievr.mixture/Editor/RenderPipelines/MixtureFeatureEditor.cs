﻿#if MIXTURE_URP
using Mixture;
using UnityEditor;
using UnityEditor.Rendering.Universal;
using UnityEngine;
using UnityEngine.Rendering.Universal;
    
[CustomEditor(typeof(MixtureFeature), true)]
public class MixtureFeatureEditor : ScriptableRendererFeatureEditor
{
    public override void OnInspectorGUI()
    {
        var settings = serializedObject.FindProperty("settings");
        var feature = serializedObject.targetObject as MixtureFeature;
        var graph = feature.settings.graphReference;

        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.PropertyField(settings.FindPropertyRelative("graph"));

            if (GUILayout.Button(new GUIContent("Open Graph"), GUILayout.Width(90)))
            {
                if (graph != null)
                    MixtureGraphWindow.Open(graph);
            }
        }

        if (graph == null)
            return;

        serializedObject.ApplyModifiedProperties();
        
        var rect = EditorGUILayout.GetControlRect(true);
        foreach (var p in graph.exposedParameters)
        {
            if (p.settings.isHidden)
                continue;

            if (MixtureRenderPipelineEditorUtils.DrawExposedParameter(p, rect))
                rect = EditorGUILayout.GetControlRect(true);
        }
        
        serializedObject.Update();
    }
}
#endif