using UnityEngine;
using GraphProcessor;

public class MixtureConversions : ITypeAdapter
{
    // Float to Vector:
    public static Vector4 ConvertFloatToVector4(float from) => new Vector4(from, from, from, from);
    public static float ConvertVector4ToFloat(Vector4 from) => from.x;
    public static Vector3 ConvertFloatToVector3(float from) => new Vector3(from, from, from);
    public static float ConvertVector3ToFloat(Vector3 from) => from.x;
    public static Vector2 ConvertFloatToVector2(float from) => new Vector2(from, from);
    public static float ConvertVector2ToFloat(Vector2 from) => from.x;

    // Colors
    public static Color ConvertVector4ToColor(Vector4 from) => new Color(from.x, from.y, from.z, from.w);
    public static Vector4 ConvertColorToVector4(Color from) => new Vector4(from.r, from.g, from.b, from.a);
    public static Color ConvertVector3ToColor(Vector3 from) => new Color(from.x, from.y, from.z, 1.0f);
    public static Vector3 ConvertColorToVector3(Color from) => new Vector3(from.r, from.g, from.b);

    // Utils function for the custom material property assignation (AssignMaterialPropertiesFromEdges)
    public static Vector4 ConvertObjectToVector4(object o)
    {
        switch (o)
        {
            case float f: return ConvertFloatToVector4(f);
            case Color c: return ConvertColorToVector4(c);
            default: return default(Vector4);
        }
    }

    public static object ConvertVector4ToObject(Vector4 v) => v;
}