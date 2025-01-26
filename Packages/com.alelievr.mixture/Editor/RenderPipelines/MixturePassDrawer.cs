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

            // TODO: Draw the parameters using ImGUI
            switch (p)
            {
                case FloatParameter r:
                    var s = r.settings as FloatParameter.FloatSettings;
                    switch (s.mode)
                    {
                        case FloatParameter.FloatMode.Default:
                            r.value = EditorGUI.FloatField(rect, r.name, (float)r.value);
                            break;
                        case FloatParameter.FloatMode.Slider:
                            r.value = EditorGUI.Slider(rect, r.name, (float)r.value, s.min, s.max);
                            break;
                        default: continue;
                    }
                    break;
                case IntParameter r:
                    var si = r.settings as IntParameter.IntSettings;
                    switch (si.mode)
                    {
                        case IntParameter.IntMode.Default:
                            r.value = EditorGUI.IntField(rect, r.name, (int)r.value);
                            break;
                        case IntParameter.IntMode.Slider:
                            r.value = EditorGUI.IntSlider(rect, r.name, (int)r.value, si.min, si.max);
                            break;
                        default: continue;
                    }
                    break;
                case ColorParameter r:
                    r.value = EditorGUI.ColorField(rect, r.name, (Color)r.value);
                    break;
                case Texture2DParameter r:
                    r.value = EditorGUI.ObjectField(rect, r.name, (Texture2D)r.value, typeof(Texture2D), false);
                    break;
                case Texture3DParameter r:
                    r.value = EditorGUI.ObjectField(rect, r.name, (Texture3D)r.value, typeof(Texture3D), false);
                    break;
                case CubemapParameter r:
                    r.value = EditorGUI.ObjectField(rect, r.name, (Cubemap)r.value, typeof(Cubemap), false);
                    break;
                case BoolParameter r:
                    r.value = EditorGUI.Toggle(rect, r.name, (bool)r.value);
                    break;
                case GradientParameter r:
                    r.value = EditorGUI.GradientField(rect, r.name, (Gradient)r.value);
                    break;
                case AnimationCurveParameter r:
                    r.value = EditorGUI.CurveField(rect, r.name, (AnimationCurve)r.value);
                    break;
                case MeshParameter r:  
                    r.value = EditorGUI.ObjectField(rect, r.name, (Mesh)r.value, typeof(Mesh), false);
                    break;
                case StringParameter r:
                    r.value = EditorGUI.TextField(rect, r.name, (string)r.value);
                    break;
                default: continue;
            }

            rect.y += lh;
            paramCount++;
        }
    }

    protected override float GetPassHeight(SerializedProperty customPass)
    {
        return lh * (paramCount + 1);
    }
}