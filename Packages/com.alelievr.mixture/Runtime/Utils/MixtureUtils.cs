using UnityEngine;
using UnityEngine.Rendering;
using System.Linq;
using System.Collections.Generic;

namespace Mixture
{
    public static class MixtureUtils
    {
		public static readonly float		defaultNodeWidth = 250f;
		public static readonly float		operatorNodeWidth = 100f;

		static Material  _blitIconMaterial;
		public static Material  blitIconMaterial
		{
			get
			{
				if (_blitIconMaterial == null)
				{
					_blitIconMaterial = new Material(Resources.Load< Shader >("MixtureIconBlit"));
					_blitIconMaterial.SetTexture("_MixtureIcon", icon);
				}

				return _blitIconMaterial;
			}
		}

		static Material  _blitRealtimeIconMaterial;
		public static Material  blitRealtimeIconMaterial
		{
			get
			{
				if (_blitRealtimeIconMaterial == null)
				{
					_blitRealtimeIconMaterial = new Material(Resources.Load< Shader >("MixtureIconBlit"));
					_blitRealtimeIconMaterial.SetTexture("_MixtureIcon", realtimeIcon);
				}

				return _blitRealtimeIconMaterial;
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

        static Material _texture2DPreviewMaterial;
        public static Material texture2DPreviewMaterial
        {
            get
            {
                if (_texture2DPreviewMaterial == null)
                {
                    _texture2DPreviewMaterial = new Material(Shader.Find("Hidden/MixtureTexture2DPreview"));
                }

                return _texture2DPreviewMaterial;
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

		static Material _dummyCustomRenderTextureMaterial;
		public static Material dummyCustomRenderTextureMaterial
		{
			get
			{
				if (_dummyCustomRenderTextureMaterial == null)
				{
					_dummyCustomRenderTextureMaterial = new Material(Shader.Find("Hidden/MixtureTextureCubePreview"));
				}

				return _dummyCustomRenderTextureMaterial;
			}
		}

        static Texture2D _windowIcon;
        public static Texture2D windowIcon
        {
            get => _windowIcon == null ? _windowIcon = Resources.Load<Texture2D>("MixtureIcon") : _windowIcon;
        }
        static Texture2D _icon;
        public static Texture2D icon
        {
            get => _icon == null ? _icon = Resources.Load<Texture2D>("MixtureIcon_128") : _icon;
        }

        static Texture2D				_icon32;
		public static Texture2D			icon32
		{
			get => _icon32 == null ? _icon32 = Resources.Load< Texture2D >("MixtureIcon_32") : _icon32;
		}
		
		static Texture2D				_realtimeIcon;
		public static Texture2D			realtimeIcon
		{
			get => _realtimeIcon == null ? _realtimeIcon = Resources.Load< Texture2D >("MixtureRealtimeIcon_128") : _realtimeIcon;
		}

		static Texture2D				_realtimeIcon32;
		public static Texture2D			realtimeIcon32
		{
			get => _realtimeIcon32 == null ? _realtimeIcon32 = Resources.Load< Texture2D >("MixtureRealtimeIcon_32") : _realtimeIcon32;
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

		static readonly Dictionary< TextureDimension, string >	shaderPropertiesDimension = new Dictionary<TextureDimension, string>{
            { TextureDimension.Tex2D, "_2D" },
            { TextureDimension.Tex3D, "_3D" },
            { TextureDimension.Cube, "_Cube" },
        };

        static readonly List< TextureDimension > allDimensions = new List<TextureDimension>() {
            TextureDimension.Tex2D, TextureDimension.Tex3D, TextureDimension.Cube,
        };

		public static List<TextureDimension> GetAllowedDimentions(string propertyName)
        {
            // if there is no modifier in the name, then it supports all the dimensions
			bool dimensionSpecific = false;
			foreach (var dim in shaderPropertiesDimension.Values)
				if (propertyName.EndsWith(dim))
					dimensionSpecific = true;

			if (!dimensionSpecific)
				return allDimensions;

            List<TextureDimension>  dimensions = new List<TextureDimension>();

            foreach (var kp in shaderPropertiesDimension)
            {
                if (propertyName.ToUpper().Contains(kp.Value.ToUpper()))
                    dimensions.Add(kp.Key);
            }

            return dimensions;
        }

        public static void DestroyGameObject(Object obj)
        {
            if (obj != null)
            {
#if UNITY_EDITOR
                if (Application.isPlaying)
                    Object.Destroy(obj);
                else
                    Object.DestroyImmediate(obj);
#else
                Object.Destroy(obj);
#endif
            }
        }
    }
}