using UnityEngine;
using UnityEngine.Rendering;
using System.Linq;

namespace Mixture
{
    public static class MixtureUtils
    {
		static Material  _blitIconMaterial;
		public static Material  blitIconMaterial
		{
			get
			{
				if (_blitIconMaterial == null)
				{
					_blitIconMaterial = new Material(Shader.Find("Hidden/MixtureIconBlit"));
					_blitIconMaterial.SetTexture("_MixtureIcon", icon);
				}

				return _blitIconMaterial;
			}
		}

		static Material	_textureArrayPreviewMaterial;
		public static Material	textureArrayPreviewMaterial
		{
			get
			{
				if (_textureArrayPreviewMaterial == null)
				{
					_textureArrayPreviewMaterial = new Material(Shader.Find("Hidden/MixtureTextureArrayPreview"));
				}

				return _textureArrayPreviewMaterial;
			}
		}

		static Material _texture3DPreviewMaterial;
		public static Material texture3DPreviewMaterial
		{
			get
			{
				if (_texture3DPreviewMaterial == null)
				{
					_texture3DPreviewMaterial = new Material(Shader.Find("Hidden/MixtureTexture3DPreview"));
				}

				return _texture3DPreviewMaterial;
			}
		}

		static Material _textureCubePreviewMaterial;
		public static Material textureCubePreviewMaterial
		{
			get
			{
				if (_textureCubePreviewMaterial == null)
				{
					_textureCubePreviewMaterial = new Material(Shader.Find("Hidden/MixtureTextureCubePreview"));
				}

				return _textureCubePreviewMaterial;
			}
		}

		static Texture2D				_icon;
		public static Texture2D			icon
		{
			get => _icon == null ? _icon = Resources.Load< Texture2D >("MixtureIcon") : _icon;
		}

		public static void SetupDimensionKeyword(Material material, TextureDimension dimension)
		{
			foreach (var keyword in material.shaderKeywords.Where(s => s.ToLower().Contains("crt")))
				material.DisableKeyword(keyword);

			switch (dimension)
			{
				case TextureDimension.Tex2D:
					material.EnableKeyword("CRT_2D");
					break;
				case TextureDimension.Tex2DArray:
					material.EnableKeyword("CRT_2D_ARRAY");
					break;
				case TextureDimension.Tex3D:
					material.EnableKeyword("CRT_3D");
					break;
				case TextureDimension.Cube:
					material.EnableKeyword("CRT_CUBE");
					break;
				default:
					break;
			}
		}
    }
}