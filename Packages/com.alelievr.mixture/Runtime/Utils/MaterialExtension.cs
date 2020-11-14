using UnityEngine;
using UnityEngine.Rendering;

namespace Mixture
{
    public static class MaterialExtension
    {
        public static void SetKeywordEnabled(this Material material, string keyword, bool enabled)
        {
            if (enabled)
                material.EnableKeyword(keyword);
            else
                material.DisableKeyword(keyword);
        }

        public static bool HasTextureBound(this Material material, string baseName, TextureDimension dimension)
        {
            var texture = material.GetTexture(baseName + MixtureUtils.shaderPropertiesDimensionSuffix[dimension]);
            return texture != null;
        }

        public static Texture GetTextureWithDimension(this Material material, string baseName, TextureDimension dimension)
        {
            return material.GetTexture(baseName + MixtureUtils.shaderPropertiesDimensionSuffix[dimension]);
        }
    }
}