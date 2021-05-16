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
        static Dictionary<TextureDimension, Texture> blackTextures = new Dictionary<TextureDimension, Texture>();
        static Dictionary<TextureDimension, Texture> whiteTextures = new Dictionary<TextureDimension, Texture>();

        // Do not change change these names, it would break all graphs that are using default texture values
        static readonly string blackDefaultTextureName = "Mixture Black";
        static readonly string whiteDefaultTextureName = "Mixture white";

        const int CurveTextureResolution = 512;

        public static Texture GetBlackTexture(TextureDimension dim, int sliceCount = 0)
        {
            Texture blackTexture;

            if (dim == TextureDimension.Any || dim == TextureDimension.Unknown || dim == TextureDimension.None)
                throw new Exception($"Unable to create white texture for type {dim}");

            if (blackTextures.TryGetValue(dim, out blackTexture))
            {
                // We don't cache texture arrays
                if (dim != TextureDimension.Tex2DArray && dim != TextureDimension.Tex2DArray)
                    return blackTexture;
            }

            blackTexture = CreateColorRenderTexture(dim, Color.black);
            blackTexture.name = blackDefaultTextureName;
            blackTextures[dim] = blackTexture;

            return blackTexture;
        }

        public static Texture GetWhiteTexture(TextureDimension dim, int sliceCount = 0)
        {
            Texture whiteTexture;

            if (dim == TextureDimension.Any || dim == TextureDimension.Unknown || dim == TextureDimension.None)
                throw new Exception($"Unable to create white texture for type {dim}");

            if (whiteTextures.TryGetValue(dim, out whiteTexture))
            {
                // We don't cache texture arrays
                if (dim != TextureDimension.Tex2DArray && dim != TextureDimension.Tex2DArray)
                    return whiteTexture;
            }

            whiteTexture = CreateColorRenderTexture(dim, Color.white);
            whiteTexture.name = whiteDefaultTextureName;
            whiteTextures[dim] = whiteTexture;

            return whiteTexture;
        }

        public static RenderTexture CreateColorRenderTexture(TextureDimension dim, Color color)
        {
            RenderTexture rt = new RenderTexture(1, 1, 0, GraphicsFormat.R8G8B8A8_UNorm, 1)
            {
                volumeDepth = 1,
                dimension = dim,
                enableRandomWrite = true,
                hideFlags = HideFlags.HideAndDontSave
            };
            var cmd = CommandBufferPool.Get();

            for (int i = 0; i < GetSliceCount(rt); i++)
            {
                cmd.SetRenderTarget(rt, 0, (CubemapFace)i, i);
                cmd.ClearRenderTarget(false, true, color);
            }

            return rt;
        }

        public static bool IsMixtureDefaultTexture(this Texture texture)
            => texture.name == blackDefaultTextureName || texture.name == whiteDefaultTextureName;

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

        static Color[] pixels = new Color[CurveTextureResolution];
        public static void UpdateTextureFromCurve(AnimationCurve curve, ref Texture2D curveTexture)
        {
            if (curveTexture == null)
            {
                curveTexture = new Texture2D(CurveTextureResolution, 1, TextureFormat.RFloat, false, true);
                curveTexture.wrapMode = TextureWrapMode.Clamp;
                curveTexture.filterMode = FilterMode.Bilinear;
                curveTexture.hideFlags = HideFlags.HideAndDontSave;
            }

            for (int i = 0; i < CurveTextureResolution; i++)
            {
                float t = (float)i / (CurveTextureResolution - 1);
                pixels[i] = new Color(curve.Evaluate(t), 0, 0, 1);
            }
            curveTexture.SetPixels(pixels);
            curveTexture.Apply(false);

        }

        public static Texture DuplicateTexture(Texture source, bool copyContent = true)
        {
            TextureCreationFlags flags = source.mipmapCount > 1 ? TextureCreationFlags.MipChain : TextureCreationFlags.None;

            switch (source)
            {
                case Texture2D t2D:
                    var new2D = new Texture2D(t2D.width, t2D.height, t2D.graphicsFormat, t2D.mipmapCount, flags);
                    CopyCommonTextureSettings(source, new2D);

                    if (copyContent)
                    {
                        for (int mipLevel = 0; mipLevel < t2D.mipmapCount; mipLevel++)
                            new2D.SetPixelData(t2D.GetPixelData<byte>(mipLevel), mipLevel);
                    }

                    return new2D;
                case Texture3D t3D:
                    var new3D = new Texture3D(t3D.width, t3D.height, t3D.depth, t3D.graphicsFormat, flags, t3D.mipmapCount);
                    CopyCommonTextureSettings(source, new3D);

                    if (copyContent)
                    {
                        for (int mipLevel = 0; mipLevel < t3D.mipmapCount; mipLevel++)
                            new3D.SetPixelData(t3D.GetPixelData<byte>(mipLevel), mipLevel);
                    }

                    return new3D;
                case Cubemap cube:
                    var newCube = new Cubemap(cube.width, cube.graphicsFormat, flags, cube.mipmapCount);
                    CopyCommonTextureSettings(source, newCube);

                    if (copyContent)
                    {
                        for (int slice = 0; slice < 6; slice++)
                            for (int mipLevel = 0; mipLevel < cube.mipmapCount; mipLevel++)
                                newCube.SetPixelData(cube.GetPixelData<byte>(mipLevel, (CubemapFace)slice), mipLevel, (CubemapFace)slice);
                    }

                    return newCube;
                case CustomRenderTexture rt:
                    var newRT = new CustomRenderTexture(rt.width, rt.height, rt.graphicsFormat);
                    newRT.dimension = rt.dimension;
                    newRT.depth = rt.depth;
                    newRT.volumeDepth = rt.volumeDepth;
                    CopyCommonTextureSettings(source, newRT);
                    newRT.enableRandomWrite = rt.enableRandomWrite;

                    if (copyContent)
                    {
                        for (int slice = 0; slice < TextureUtils.GetSliceCount(rt); slice++)
                            for (int mipLevel = 0; mipLevel < rt.mipmapCount; mipLevel++)
                                Graphics.CopyTexture(rt, slice, mipLevel, newRT, slice, mipLevel);
                    }

                    return newRT;
                default:
                    throw new System.Exception("Can't duplicate texture of type " + source.GetType());
            }

            void CopyCommonTextureSettings(Texture source, Texture destination)
            {
                destination.name = source.name;
                destination.wrapMode = source.wrapMode;
                destination.filterMode = source.filterMode;
                destination.wrapModeU = source.wrapModeU;
                destination.wrapModeV = source.wrapModeV;
                destination.wrapModeW = source.wrapModeW;
                destination.anisoLevel = source.anisoLevel;
            }
        }

        public static void CopyTexture(Texture source, Texture destination, bool copyMips = true)
        {
            var cmd = CommandBufferPool.Get("CopyTexture");
            CopyTexture(cmd, source, destination, copyMips);
            Graphics.ExecuteCommandBuffer(cmd);
        }

        static ProfilingSampler copyTextureSampler = new ProfilingSampler("Copy Texture");
        public static void CopyTexture(CommandBuffer cmd, Texture source, Texture destination, bool copyMips = true)
        {
            int mipStop = copyMips ? source.mipmapCount : 1;
            CopyTexture(cmd, source, destination, 0, mipStop);
        }

        public static void CopyTexture(CommandBuffer cmd, Texture source, Texture destination, int mipStart, int mipStop = -1)
        {
            if (mipStop == -1)
                mipStop = mipStart + 1;
            using (new ProfilingScope(cmd, new ProfilingSampler("Copy Texture " + source.name + " to " + destination.name)))
            {
                int originalSliceCount = (source.dimension == TextureDimension.Cube) ? 6 : TextureUtils.GetSliceCount(source);

                bool canCopy = source.graphicsFormat == destination.graphicsFormat && source.width == destination.width && source.height == destination.height;

                if (canCopy)
                {
                    for (int mipLevel = mipStart; mipLevel < mipStop; mipLevel++)
                    {
                        int sliceCount = source.dimension == TextureDimension.Tex3D ? Mathf.Max(originalSliceCount >> mipLevel, 1) : originalSliceCount;
                        for (int slice = 0; slice < sliceCount; slice++)
                        {
                            // CopyTexture with mip API is really weird, it needs to take the slice << mipLevel otherwise it doesn't work
                            int copySlice = source.dimension == TextureDimension.Tex3D ? slice << mipLevel : slice;
                            cmd.CopyTexture(source, copySlice, mipLevel, destination, copySlice, mipLevel);
                        }
                    }

                }
                else
                {
                    // no mips mip target in Blit call
                    for (int slice = 0; slice < originalSliceCount; slice++)
                        cmd.Blit(source, destination, slice, 0);
                }
            }
        }
    }
}