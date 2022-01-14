using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;
using System;
using UnityEngine.Serialization;

namespace Mixture
{
	[Documentation(@"
The Texture node can accept any type of texture in parameter (2D, 3D, 2DArray, Cube, CubeArray, RenderTexture).
The output type of the node will update according to the type of texture provided. In case the texture type changes, the output edges may be destroyed.
")]

	[System.Serializable, NodeMenuItem("Constants/Texture")]
	public class TextureNode : MixtureNode, ICreateNodeFrom<Texture> 
	{
		public enum PowerOf2Mode
		{
			None,
			ScaleToNextPowerOf2,
			ScaleToClosestPowerOf2,
		}

		[SerializeField, FormerlySerializedAs("texture")]
		public Texture textureAsset;
		[Output(name = "Texture")]
		public Texture outputTexture;

        public override bool canEditPreviewSRGB => !IsInputImportedTexture();

        public override bool 	hasSettings => false;
		public override string	name => "Texture";
        public override Texture previewTexture => outputTexture;
		public override bool	showDefaultInspector => true;
        public override bool	isRenamable => true;

		public PowerOf2Mode		POTMode = PowerOf2Mode.None;

		[SerializeField, HideInInspector]
		bool normalMap = false;

		[NonSerialized]
		RenderTexture			postProcessedTexture = null;

		[CustomPortBehavior(nameof(outputTexture))]
		IEnumerable<PortData> OutputTextureType(List<SerializableEdge> edges)
		{
			var dim = textureAsset == null ? settings.GetResolvedTextureDimension(graph) : textureAsset is RenderTexture rt ? rt.dimension : textureAsset?.dimension;
			yield return new PortData
			{
				displayName = "Texture",
				displayType = dim == null ? typeof(Texture) : TextureUtils.GetTypeFromDimension(dim.Value),
				identifier = nameof(outputTexture),
				acceptMultipleEdges = true,
			};
		}

		public bool IsPowerOf2(Texture t)
		{
			bool isPOT = false;

			if (!Mathf.IsPowerOfTwo(t.width))
				return false;

			// Check if texture is POT
			if (t.dimension == TextureDimension.Tex2D)
				isPOT = t.width == t.height;
			else if (t.dimension == TextureDimension.Cube)
				isPOT = true;
			else if (t.dimension == TextureDimension.Tex3D)
				isPOT = t.width == t.height && t.width == TextureUtils.GetSliceCount(t);

			return isPOT;
		}

        protected override bool ProcessNode(CommandBuffer cmd)
        {
            if (!base.ProcessNode(cmd) || textureAsset == null)
				return false;

#if UNITY_EDITOR
			var importer = UnityEditor.AssetImporter.GetAtPath(UnityEditor.AssetDatabase.GetAssetPath(textureAsset));
			if (importer is UnityEditor.TextureImporter textureImporter)
            {
				normalMap = textureImporter.textureType == UnityEditor.TextureImporterType.NormalMap;
				previewSRGB = textureImporter.sRGBTexture;
			}
#endif

			int targetWidth = textureAsset.width;
			int targetHeight = textureAsset.height;
			int targetDepth = TextureUtils.GetSliceCount(textureAsset);
			bool needsTempTarget = false;
			if (!IsPowerOf2(textureAsset) && POTMode != PowerOf2Mode.None)
			{
				int maxSize = Mathf.Max(Mathf.Max(targetWidth, targetHeight), targetDepth);
				int potSize = 0;

				switch (POTMode)
				{
					case PowerOf2Mode.ScaleToNextPowerOf2:
						potSize = Mathf.NextPowerOfTwo(maxSize);
						break;
					default:
						potSize = Mathf.ClosestPowerOfTwo(maxSize);
						break;
				}
				targetWidth = targetHeight = targetDepth = potSize;
				needsTempTarget = true;
			}
			if (normalMap)
				needsTempTarget = true;

			if (needsTempTarget && postProcessedTexture == null)
				postProcessedTexture = new RenderTexture(1, 1, 0, GraphicsFormat.R16G16B16A16_SFloat, mipCount: textureAsset.mipmapCount) { dimension = textureAsset.dimension, enableRandomWrite = true, volumeDepth = 1};
			else if (!needsTempTarget)
			{
				postProcessedTexture?.Release();
				postProcessedTexture = null;
			}

			if (postProcessedTexture != null && (postProcessedTexture.width != targetWidth 
				|| postProcessedTexture.height != targetHeight
				|| postProcessedTexture.volumeDepth != targetDepth))
			{
				postProcessedTexture.Release();
				postProcessedTexture.width = targetWidth;
				postProcessedTexture.height = targetHeight;
				postProcessedTexture.volumeDepth = targetDepth;
				postProcessedTexture.Create();
			}
			// TODO: same alloc as normal map + scale and crop options

			// Compressed normal maps need to be converted from AG to RG format
			if (normalMap)
			{
				// Transform normal map texture into POT
				var blitMaterial = GetTempMaterial("Hidden/Mixture/TextureNode");
				MixtureUtils.SetTextureWithDimension(blitMaterial, "_Source", textureAsset);
				blitMaterial.SetInt("_POTMode", (int)POTMode);
				MixtureUtils.Blit(cmd, blitMaterial, textureAsset, postProcessedTexture, 0);

				outputTexture = postProcessedTexture;
			}
			else if (needsTempTarget)
			{
				// Transform standard texture into POT
				var blitMaterial = GetTempMaterial("Hidden/Mixture/TextureNode");
				MixtureUtils.SetTextureWithDimension(blitMaterial, "_Source", textureAsset);
				blitMaterial.SetInt("_POTMode", (int)POTMode);
				MixtureUtils.Blit(cmd, blitMaterial, textureAsset, postProcessedTexture, 1);

				outputTexture = postProcessedTexture;
			}
			else
			{
				outputTexture = textureAsset;
			}

			if (outputTexture != null)
			{
				settings.sizeMode = OutputSizeMode.Absolute;
				settings.width = outputTexture.width;
				settings.height = outputTexture.height;
				settings.depth = TextureUtils.GetSliceCount(outputTexture);
			}

			return true;
        }

		bool IsInputImportedTexture()
        {
#if UNITY_EDITOR
			var importer = UnityEditor.AssetImporter.GetAtPath(UnityEditor.AssetDatabase.GetAssetPath(textureAsset));
			return importer != null && importer is UnityEditor.TextureImporter;
#else
			return false;
#endif
		}

		public bool InitializeNodeFromObject(Texture value)
		{
			textureAsset = value;
			return true;
		}
    }
}