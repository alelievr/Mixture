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

    // Vector to Vector:
    public static Vector2 ConvertVector4ToVector2(Vector4 v) => new Vector2(v.x, v.y);
    public static Vector4 ConvertVector2ToVector4(Vector2 from) => new Vector4(from.x, from.y, 0, 0);
    public static Vector3 ConvertVector4ToVector3(Vector4 v) => new Vector3(v.x, v.y, v.z);
    public static Vector4 ConvertVector3ToVector4(Vector3 from) => new Vector4(from.x, from.y, from.z, 0);
    public static Vector2 ConvertVector3ToVector2(Vector3 v) => new Vector2(v.x, v.y);
    public static Vector3 ConvertVector2ToVector3(Vector2 from) => new Vector3(from.x, from.y, 0);
    
    public static Vector2Int ConvertVector3IntToVector2Int(Vector3Int v) => new Vector2Int(v.x, v.y);
    public static Vector3Int ConvertVector2IntToVector3Int(Vector2Int from) => new Vector3Int(from.x, from.y, 0);

    public static Vector2 ConvertVector2IntToVector2(Vector2Int from) => new Vector2(from.x, from.y);
    public static Vector2Int ConvertVector2ToVector2Int(Vector2 from) => new Vector2Int((int)from.x, (int)from.y);
    public static Vector3 ConvertVector3IntToVector3(Vector3Int from) => new Vector3(from.x, from.y, from.z);
    public static Vector3Int ConvertVector3ToVector3Int(Vector3 from) => new Vector3Int((int)from.x, (int)from.y, (int)from.z);

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

    public static Color ConvertObjectToColor(object o)
    {
        switch (o)
        {
            case Vector3 v: return ConvertVector3ToColor(v);
            case Vector4 v: return ConvertVector4ToColor(v);
            case Color c: return c;
            default: return Color.black;
        }
    }

    public static object ConvertColorToObject(Color c) => c;
}