using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using System.IO;
using System;
using UnityEngine.Rendering;

using UnityEngine.Experimental.Rendering;

namespace Mixture
{
    public static class TextureUtils
    {
        static Dictionary< TextureDimension, Texture >  blackTextures = new Dictionary< TextureDimension, Texture >();
        static Dictionary< TextureDimension, Texture >  whiteTextures = new Dictionary< TextureDimension, Texture >();

        public static Texture GetBlackTexture(MixtureRTSettings settings)
        {
            return GetBlackTexture((TextureDimension)settings.dimension, settings.sliceCount);
        }

        public static Texture GetBlackTexture(TextureDimension dim, int sliceCount = 0)
        {
            Texture blackTexture;

            if (blackTextures.TryGetValue(dim, out blackTexture))
            {
                // We don't cache texture arrays
                if (dim != TextureDimension.Tex2DArray && dim != TextureDimension.Tex2DArray)
                    return blackTexture;
            }

            switch (dim)
            {
                case TextureDimension.Tex2D:
                    blackTexture = Texture2D.blackTexture;
                    break ;
                case TextureDimension.Tex3D:
                    blackTexture = new Texture3D(1, 1, 1, TextureFormat.RGBA32, 0);
                    (blackTexture as Texture3D).SetPixels(new []{Color.black});
                    (blackTexture as Texture3D).Apply();
                    break ;
                case TextureDimension.Tex2DArray:
                    blackTexture = new Texture2DArray(1, 1, sliceCount, TextureFormat.RGBA32, 0, true);
                    for (int i = 0; i < sliceCount; i++)
                        (blackTexture as Texture2DArray).SetPixels(new []{Color.black}, i);
                    (blackTexture as Texture2DArray).Apply();
                    break ;
                case TextureDimension.Cube:
                    blackTexture = new Cubemap(1, TextureFormat.RGBA32, 0);
                    for (int i = 0; i < 6; i++)
                        (blackTexture as Cubemap).SetPixel((CubemapFace)i, 0, 0, Color.black);
                    (blackTexture as Cubemap).Apply();
                    break ;
                default: // TextureDimension.Any / TextureDimension.Unknown
                    throw new Exception($"Unable to create black texture for type {dim}");
            }

            blackTexture.name = "Mixture Black";
            blackTextures[dim] = blackTexture;

            return blackTexture;
        }

        public static Texture GetWhiteTexture(TextureDimension dim, int sliceCount = 0)
        {
            Texture whiteTexture;

            if (whiteTextures.TryGetValue(dim, out whiteTexture))
            {
                // We don't cache texture arrays
                if (dim != TextureDimension.Tex2DArray && dim != TextureDimension.Tex2DArray)
                    return whiteTexture;
            }

            switch (dim)
            {
                case TextureDimension.Tex2D:
                    whiteTexture = Texture2D.whiteTexture;
                    break ;
                case TextureDimension.Tex3D:
                    whiteTexture = new Texture3D(1, 1, 1, TextureFormat.RGBA32, 0);
                    (whiteTexture as Texture3D).SetPixels(new []{Color.white});
                    (whiteTexture as Texture3D).Apply();
                    break ;
                case TextureDimension.Tex2DArray:
                    whiteTexture = new Texture2DArray(1, 1, sliceCount, TextureFormat.RGBA32, 0, true);
                    for (int i = 0; i < sliceCount; i++)
                        (whiteTexture as Texture2DArray).SetPixels(new []{Color.white}, i);
                    (whiteTexture as Texture2DArray).Apply();
                    break ;
                case TextureDimension.Cube:
                    whiteTexture = new Cubemap(1, TextureFormat.RGBA32, 0);
                    for (int i = 0; i < 6; i++)
                        (whiteTexture as Cubemap).SetPixel((CubemapFace)i, 0, 0, Color.white);
                    break ;
                default: // TextureDimension.Any / TextureDimension.Unknown
                    throw new Exception($"Unable to create white texture for type {dim}");
            }

            whiteTexture.name = "Mixture white";
            whiteTextures[dim] = whiteTexture;

            return whiteTexture;
        }

        public static int GetSliceCount(Texture tex)
        {
            if (tex == null)
                return 0;

            switch (tex)
            {
                case Texture2D _:
                    return 1;
                case Texture2DArray t:
                    return t.depth;
                case Texture3D t:
                    return t.depth;
                case CubemapArray t:
                    return t.cubemapCount;
                case Cubemap _:
                    return 1;
                case RenderTexture rt:
                    if (rt.dimension == TextureDimension.Tex2D || rt.dimension == TextureDimension.Cube)
                        return 1;
                    else if (rt.dimension == TextureDimension.Tex3D || rt.dimension == TextureDimension.Tex2DArray || rt.dimension == TextureDimension.CubeArray)
                        return rt.volumeDepth;
                    else
                        return 0;
                default:
                    return 0;
            }
        }

        public static Type GetTypeFromDimension(TextureDimension dimension)
        {
            switch (dimension)
            {
                case TextureDimension.Tex2D:
                    return typeof(Texture2D);
                case TextureDimension.Tex2DArray:
                    return typeof(Texture2DArray);
                case TextureDimension.Tex3D:
                    return typeof(Texture3D);
                case TextureDimension.Cube:
                    return typeof(Cubemap);
                case TextureDimension.CubeArray:
                    return typeof(CubemapArray);
                default:
                    return typeof(Texture);
            }
        }

        public static TextureDimension GetDimensionFromType(Type textureType)
        {
            if (textureType == typeof(Texture2D))
                return TextureDimension.Tex2D;
            else if (textureType == typeof(Texture2DArray))
                return TextureDimension.Tex2DArray;
            else if (textureType == typeof(Texture3D))
                return TextureDimension.Tex3D;
            else if (textureType == typeof(Cubemap))
                return TextureDimension.Cube;
            else if (textureType == typeof(CubemapArray))
                return TextureDimension.CubeArray;
            else
                return TextureDimension.Unknown;
        }
    }
}