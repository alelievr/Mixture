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

    // Float to int:
    public static int ConvertFloatToInt(float from) => Mathf.RoundToInt(from);
    public static float ConvertIntToFloat(int from) => (float)from;

    // Int to vector
    public static Vector4 ConvertIntToVector4(int from) => new Vector4(from, from, from, from);
    public static int ConvertVector4ToInt(Vector4 from) => (int)from.x;
    public static Vector3 ConvertIntToVector3(int from) => new Vector3(from, from, from);
    public static int ConvertVector3ToInt(Vector3 from) => (int)from.x;
    public static Vector2 ConvertIntToVector2(int from) => new Vector2(from, from);
    public static int ConvertVector2ToInt(Vector2 from) => (int)from.x;

    // Utils function for the custom material property assignation (AssignMaterialPropertiesFromEdges)
    public static Vector4 ConvertObjectToVector4(object o)
    {
        switch (o)
        {
            case float f: return ConvertFloatToVector4(f);
            case Color c: return ConvertColorToVector4(c);
            case Vector4 v: return v;
            default: return default(Vector4);
        }
    }

    public static object ConvertVector4ToObject(Vector4 v) => v;
}