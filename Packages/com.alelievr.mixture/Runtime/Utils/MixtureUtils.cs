using UnityEngine;
using UnityEngine.Rendering;
using System.Linq;
using System.Collections.Generic;
using GraphProcessor;

namespace Mixture
{
    public static class MixtureUtils
    {
		public static readonly float		defaultNodeWidth = 250f;
		public static readonly float		operatorNodeWidth = 100f;
		public static readonly float		smallNodeWidth = 150f;

		static Material  _blitIconMaterial;
		public static Material  blitIconMaterial
		{
			get
			{
				if (_blitIconMaterial == null)
				{
					_blitIconMaterial = new Material(Resources.Load< Shader >("MixtureIconBlit"));
					_blitIconMaterial.SetTexture("_MixtureIcon", icon);
					_blitIconMaterial.hideFlags = HideFlags.HideAndDontSave;
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
					_blitRealtimeIconMaterial.hideFlags = HideFlags.HideAndDontSave;
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
					var shader = Shader.Find("Hidden/CustomRenderTextureMissingMaterial");

					// The shader can be null if a mixture is called while the package is imported
					if (shader != null)
						_dummyCustomRenderTextureMaterial = new Material(shader);
				}

				return _dummyCustomRenderTextureMaterial;
			}
		}

        static Texture2D _windowIcon;
        public static Texture2D windowIcon
        {
            get => _windowIcon == null ? _windowIcon = Resources.Load<Texture2D>("Icons/MixtureIcon") : _windowIcon;
        }
        static Texture2D _icon;
        public static Texture2D icon
        {
            get => _icon == null ? _icon = Resources.Load<Texture2D>("Icons/MixtureIcon_128") : _icon;
        }

        static Texture2D				_icon32;
		public static Texture2D			icon32
		{
			get => _icon32 == null ? _icon32 = Resources.Load< Texture2D >("Icons/MixtureIcon_32") : _icon32;
		}
		
		static Texture2D				_realtimeIcon;
		public static Texture2D			realtimeIcon
		{
			get => _realtimeIcon == null ? _realtimeIcon = Resources.Load< Texture2D >("Icons/MixtureRealtimeIcon_128") : _realtimeIcon;
		}

		static Texture2D				_realtimeIcon32;
		public static Texture2D			realtimeIcon32
		{
			get => _realtimeIcon32 == null ? _realtimeIcon32 = Resources.Load< Texture2D >("Icons/MixtureRealtimeIcon_32") : _realtimeIcon32;
		}

		static ComputeShader			_clearCompute;
		public static ComputeShader		clearCompute
		{
			get => _clearCompute == null ? _clearCompute = Resources.Load< ComputeShader >("Mixture/Clear") : _clearCompute;
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

		public static void SetupComputeDimensionKeyword(ComputeShader computeShader, TextureDimension dimension)
		{
			computeShader.DisableKeyword("CRT_2D");
			computeShader.DisableKeyword("CRT_3D");
			if (dimension == TextureDimension.Tex2D)
				computeShader.EnableKeyword("CRT_2D");
			else if (dimension == TextureDimension.Tex3D)
				computeShader.EnableKeyword("CRT_3D");
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
			{
				// if (string.Compare(propertyName, propertyName.Length - dim.Length, dim, 0, dim.Length) == 0)
				if (propertyName.EndsWith(dim))
					dimensionSpecific = true;
			}

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

		static readonly int clearLimitId = Shader.PropertyToID("_ClearLimit");
		static readonly int offsetId = Shader.PropertyToID("_Offset");
		static readonly int rawId = Shader.PropertyToID("_Raw");

		/// <summary>
		/// Beware, this function is generic and slow :(
		/// </summary>
		public static void ClearBuffer(CommandBuffer cmd, ComputeBuffer buffer, int size = -1, int offset = 0)
		{
			if (size == -1)
				size = buffer.count	* buffer.stride / 4;

			cmd.SetComputeIntParam(clearCompute, offsetId, offset);
			cmd.SetComputeIntParam(clearCompute, clearLimitId, size);
			cmd.SetComputeBufferParam(clearCompute, 0, rawId, buffer);
			int x = Mathf.Clamp(size / 128, 1, 128);
			int y = Mathf.Max(size / 4096, 1);
			cmd.DispatchCompute(clearCompute, 0, x, y, 1);
		}

		public static PortData UpdateInputPortType(ref SerializableType type, string displayName, List<SerializableEdge> edges)
		{
            if (edges.Count > 0)
                type.type = edges[0].outputPort.portData.displayType ?? edges[0].outputPort.fieldInfo.FieldType;
			else
				type.type = typeof(object);

            return new PortData
            {
                identifier = displayName,
                displayName = displayName,
                acceptMultipleEdges = false,
                displayType = type.type,
            };
		}

		public static void SetupIsSRGB(Material material, MixtureNode node, MixtureGraph graph)
		{
			bool srgb = node.rtSettings.GetOutputPrecision(graph) == OutputPrecision.SRGB;

			// Output node already have a conversion on the crt level
			material.SetInt("_IsSRGB", srgb && !(node is OutputNode) ? 1: 0);
		}
    }
}