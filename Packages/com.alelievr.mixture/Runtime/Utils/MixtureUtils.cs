using UnityEngine;
using UnityEngine.Rendering;
using System.Linq;
using System.Collections.Generic;
using GraphProcessor;
using System;
using Object = UnityEngine.Object;

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

		static Material _rasterize3DMaterial;
		public static Material rasterize3DMaterial
		{
			get
			{
				if (_rasterize3DMaterial == null)
				{
					var shader = Shader.Find("Hidden/Mixture/Rasterize3D");

					// The shader can be null if a mixture is called while the package is imported
					if (shader != null)
					{
						_rasterize3DMaterial = new Material(shader);
					}
				}

				_rasterize3DMaterial.SetFloat("_ConservativeRaster", 0);
				return _rasterize3DMaterial;
			}
		}

		// Conservative rasterization needs to be enabled on the material instead of on command buffer
		static Material _rasterize3DMaterialConservative;
		public static Material rasterize3DMaterialConservative
		{
			get
			{
				if (_rasterize3DMaterialConservative == null)
				{
					var shader = Shader.Find("Hidden/Mixture/Rasterize3D");

					// The shader can be null if a mixture is called while the package is imported
					if (shader != null)
					{
						_rasterize3DMaterialConservative = new Material(shader);
					}
				}

				_rasterize3DMaterialConservative.SetFloat("_ConservativeRaster", 1);
				return _rasterize3DMaterialConservative;
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

		static Texture2D _iconVariant;
        public static Texture2D iconVariant
        {
            get => _iconVariant == null ? _iconVariant = Resources.Load<Texture2D>("Icons/MixtureVariantIcon_128") : _iconVariant;
        }

        static Texture2D				_iconVariant32;
		public static Texture2D			iconVariant32
		{
			get => _iconVariant32 == null ? _iconVariant32 = Resources.Load< Texture2D >("Icons/MixtureVariantIcon_32") : _iconVariant32;
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

		static Texture2D				_realtimeVariantIcon;
		public static Texture2D			realtimeVariantIcon
		{
			get => _realtimeVariantIcon == null ? _realtimeVariantIcon = Resources.Load< Texture2D >("Icons/MixtureRealtimeVariantIcon_128") : _realtimeVariantIcon;
		}

		static Texture2D				_realtimeVariantIcon32;
		public static Texture2D			realtimeVariantIcon32
		{
			get => _realtimeVariantIcon32 == null ? _realtimeVariantIcon32 = Resources.Load< Texture2D >("Icons/MixtureRealtimeVariantIcon_32") : _realtimeVariantIcon32;
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

		static int textureDimensionShaderId = Shader.PropertyToID("_TextureDimension");
		public static void SetupComputeTextureDimension(CommandBuffer cmd, ComputeShader computeShader, TextureDimension dimension)
		{
			cmd.SetComputeFloatParam(computeShader, textureDimensionShaderId, (int)dimension);
		}

		public static readonly string texture2DPrefix = "_2D";
		public static readonly string texture3DPrefix = "_3D";
		public static readonly string textureCubePrefix = "_Cube";

		public static readonly Dictionary< TextureDimension, string >	shaderPropertiesDimensionSuffix = new Dictionary<TextureDimension, string>{
            { TextureDimension.Tex2D, texture2DPrefix },
            { TextureDimension.Tex3D, texture3DPrefix },
            { TextureDimension.Cube, textureCubePrefix },
        };

        static readonly List< TextureDimension > allDimensions = new List<TextureDimension>() {
            TextureDimension.Tex2D, TextureDimension.Tex3D, TextureDimension.Cube,
        };

		public static List<TextureDimension> GetAllowedDimentions(string propertyName)
        {
            // if there is no modifier in the name, then it supports all the dimensions
			bool dimensionSpecific = false;
			foreach (var dim in shaderPropertiesDimensionSuffix.Values)
			{
				// if (string.Compare(propertyName, propertyName.Length - dim.Length, dim, 0, dim.Length) == 0)
				if (propertyName.EndsWith(dim))
					dimensionSpecific = true;
			}

			if (!dimensionSpecific)
				return allDimensions;

            List<TextureDimension>  dimensions = new List<TextureDimension>();

            foreach (var kp in shaderPropertiesDimensionSuffix)
            {
                if (propertyName.ToUpper().Contains(kp.Value.ToUpper()))
                    dimensions.Add(kp.Key);
            }

            return dimensions;
        }

		public static void SetTextureWithDimension(Material material, string propertyName, Texture texture)
		{
			if (shaderPropertiesDimensionSuffix.TryGetValue(texture.dimension, out var suffix))
			{
#if UNITY_EDITOR
				int id = material.shader.FindPropertyIndex(propertyName + suffix);

				if (id == -1)
					return;

				if (material.shader.GetPropertyTextureDimension(id) == texture.dimension)
#endif
					material.SetTexture(propertyName + suffix, texture);
			}
		}

		public static void SetTextureWithDimension(MaterialPropertyBlock block, string propertyName, Texture texture)
		{
			if (shaderPropertiesDimensionSuffix.TryGetValue(texture.dimension, out var suffix))
				block.SetTexture(propertyName + suffix, texture);
		}

		public static void SetTextureWithDimension(CommandBuffer cmd, ComputeShader compute, int kernelIndex, string propertyName, Texture texture)
		{
			foreach (var dim in shaderPropertiesDimensionSuffix)
			{
				if (dim.Key == texture.dimension)
					cmd.SetComputeTextureParam(compute, kernelIndex, propertyName + dim.Value, texture);
				else // We still need to bind something to the other texture dimension to avoid errors in the console
					cmd.SetComputeTextureParam(compute, kernelIndex, propertyName + dim.Value, TextureUtils.GetBlackTexture(dim.Key));
			}
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

        public static T GetLastEnumValue<T>() where T : Enum 
            => typeof(T).GetEnumValues().Cast<T>().Last();
		
		public static void Blit(CommandBuffer cmd, Material material, Texture source, RenderTexture target, int shaderPass = 0)
		{
			int sliceCount = source.dimension == TextureDimension.Cube ? 6 : TextureUtils.GetSliceCount(source);
			var props = new MaterialPropertyBlock();

			// Patch up custom render texture data for Blit
			props.SetVectorArray("CustomRenderTextureSizesAndRotations", new List<Vector4>(){new Vector4(1, 1, 0, 0)});
			props.SetVectorArray("CustomRenderTextureCenters", new List<Vector4>(){new Vector4(0.5f, 0.5f, 0, 0)});

			for (int i = 0; i < sliceCount; i++)
			{
				for (int mip = 0; i < source.mipmapCount; i++)
				{
					CubemapFace face = source.dimension == TextureDimension.Cube ? (CubemapFace)i : CubemapFace.Unknown;
					int depthSlice = source.dimension == TextureDimension.Tex3D ? i : 0;
					props.SetFloat("_Mip", mip);
					cmd.SetRenderTarget(target, mip, face, depthSlice);
					cmd.DrawProcedural(Matrix4x4.identity, material, shaderPass, MeshTopology.Triangles, 6, 1, props);
				}
			}
		}

		public static void RasterizeMeshToTexture3D(CommandBuffer cmd, MixtureMesh mesh, RenderTexture outputVolume, bool conservative = false)
		{
			var props = new MaterialPropertyBlock();

			var material = conservative ? rasterize3DMaterialConservative : rasterize3DMaterial;
			props.SetVector("_OutputSize", new Vector4(outputVolume.width, 1.0f / (float)outputVolume.width));
			cmd.SetRandomWriteTarget(2, outputVolume);
			cmd.GetTemporaryRT(42, (int)outputVolume.width, (int)outputVolume.height, 0);
			cmd.SetRenderTarget(42);
			RenderMesh(Quaternion.Euler(90, 0, 0));
			RenderMesh(Quaternion.Euler(0, 90, 0));
			RenderMesh(Quaternion.Euler(0, 0, 90));
			cmd.ClearRandomWriteTargets();

			void RenderMesh(Quaternion cameraRotation)
			{
				var worldToCamera = Matrix4x4.Rotate(cameraRotation);
				var projection = Matrix4x4.Ortho(-1, 1, -1, 1, -1, 1);
				var vp = projection * worldToCamera;
				props.SetMatrix("_CameraMatrix", vp);
				cmd.DrawMesh(mesh.mesh, mesh.localToWorld, material, 0, shaderPass: 0, props);
			}
		}
    }
}