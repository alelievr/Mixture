using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System;
using System.Linq;
using UnityEngine.Rendering;

namespace Mixture
{
    [Serializable]
    public class OutputTextureSettings
    {
        public Texture						inputTexture = null;
        public string						name = "Input #";
        public bool							enableCompression = true;
        public TextureFormat		        compressionFormat = TextureFormat.DXT5;
        public MixtureCompressionQuality	compressionQuality = MixtureCompressionQuality.Best;
        public bool							hasMipMaps = false;
        public bool                         isMain = false;

        public bool                         enableConversion = false;
        public ConversionFormat             conversionFormat;

        public bool                         sRGB = false;

        public Material						finalCopyMaterial = null;
        [NonSerialized]
        public CustomRenderTexture			finalCopyRT = null;

        public enum Preset
        {
            Color,
            Raw,
            Normal,
            Height,
            MaskHDRP,
            DetailHDRP,
            DetailURP,
        }

        public bool IsCompressionEnabled()
            => enableCompression && (finalCopyRT.dimension == TextureDimension.Tex2D || finalCopyRT.dimension == TextureDimension.Cube);

        public bool IsConversionEnabled()
            => enableConversion && (finalCopyRT.dimension == TextureDimension.Tex3D || finalCopyRT.dimension == TextureDimension.Cube);

        public void SetupPreset(Preset preset, Func<string, string> getUniqueName)
        {
            switch (preset)
            {
                case Preset.Color:
                    name = getUniqueName("Color");
                    compressionFormat = TextureFormat.BC7;
                    sRGB = true;
                    break;
                case Preset.Raw:
                    name = getUniqueName("Output");
                    enableCompression = false;
                    break;
                case Preset.Normal:
                    name = getUniqueName("Normal");
                    compressionFormat = TextureFormat.BC5;
                    break;
                case Preset.Height:
                    name = getUniqueName("Height");
                    compressionFormat = TextureFormat.BC4;
                    break;
                case Preset.MaskHDRP:
                    name = getUniqueName("Mask (HDRP)");
                    compressionFormat = TextureFormat.BC7;
                    break;
                case Preset.DetailHDRP:
                    name = getUniqueName("Detail (HDRP)");
                    compressionFormat = TextureFormat.BC7;
                    break;
                case Preset.DetailURP:
                    // TODO
                    break;
            }
        }
    }
}