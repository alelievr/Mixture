using UnityEngine;

namespace Mixture
{
    /// <summary>
    /// List all the TextureFormat that can be used in a Graphics.ConvertTexture operation
    /// </summary>
    public enum ConversionFormat
    {
        Alpha8 = TextureFormat.Alpha8,
        ARGB4444 = TextureFormat.ARGB4444,
        RGB24 = TextureFormat.RGB24,
        RGBA32 = TextureFormat.RGBA32,
        ARGB32 = TextureFormat.ARGB32,
        RGB565 = TextureFormat.RGB565,
        R16 = TextureFormat.R16,
        RGBA4444 = TextureFormat.RGBA4444,
        BGRA32 = TextureFormat.BGRA32,
        RHalf = TextureFormat.RHalf,
        RGHalf = TextureFormat.RGHalf,
        RGBAHalf = TextureFormat.RGBAHalf,
        RFloat = TextureFormat.RFloat,
        RGFloat = TextureFormat.RGFloat,
        RGBAFloat = TextureFormat.RGBAFloat,
        RG16 = TextureFormat.RG16,
        R8 = TextureFormat.R8,
    }
}