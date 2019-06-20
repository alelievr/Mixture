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
                    blackTexture = new Texture3D(1, 1, 1, DefaultFormat.HDR, TextureCreationFlags.None);
                    (blackTexture as Texture3D).SetPixels(new []{Color.black});
                    (blackTexture as Texture3D).Apply();
                    break ;
                case TextureDimension.Tex2DArray:
                    blackTexture = new Texture2DArray(1, 1, sliceCount, DefaultFormat.HDR, TextureCreationFlags.None);
                    for (int i = 0; i < sliceCount; i++)
                        (blackTexture as Texture2DArray).SetPixels(new []{Color.black}, i);
                    (blackTexture as Texture2DArray).Apply();
                    break ;
                default: // TextureDimension.Any / TextureDimension.Unknown
                    throw new Exception($"Unable to create black texture for type {dim}");
            }

            blackTextures[dim] = blackTexture;

            return blackTexture;
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
                    return rt.volumeDepth;
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
    }
}