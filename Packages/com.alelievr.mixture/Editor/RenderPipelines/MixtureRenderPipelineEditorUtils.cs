using GraphProcessor;
using UnityEngine;
using Mixture;
using UnityEditor;

public static class MixtureRenderPipelineEditorUtils
{
    public static bool DrawExposedParameter(ExposedParameter p, Rect rect)
    {
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
                    default: return false;
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
                    default: return false;
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
            default: return false;
        }

        return true;
    }
}
