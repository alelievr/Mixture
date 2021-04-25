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
		[SerializeField, FormerlySerializedAs("texture")]
		public Texture textureAsset;
		[Output(name = "Texture")]
		public Texture outputTexture;

		public override bool 	hasSettings => false;
		public override string	name => "Texture";
        public override Texture previewTexture => outputTexture;
		public override bool	showDefaultInspector => true;
        public override bool	isRenamable => true;

		[SerializeField, HideInInspector]
		bool normalMap = false;

		[NonSerialized]
		RenderTexture			postProcessedTexture = null;

		[CustomPortBehavior(nameof(outputTexture))]
		IEnumerable<PortData> OutputTextureType(List<SerializableEdge> edges)
		{
			var dim = textureAsset == null ? rtSettings.GetTextureDimension(graph) : textureAsset is RenderTexture rt ? rt.dimension : textureAsset?.dimension;
			yield return new PortData
			{
				displayName = "Texture",
				displayType = dim == null ? typeof(Texture) : TextureUtils.GetTypeFromDimension(dim.Value),
				identifier = nameof(outputTexture),
				acceptMultipleEdges = true,
			};
		}

        protected override bool ProcessNode(CommandBuffer cmd)
        {
            if (!base.ProcessNode(cmd) || textureAsset == null)
				return false;

#if UNITY_EDITOR
			var importer = UnityEditor.AssetImporter.GetAtPath(UnityEditor.AssetDatabase.GetAssetPath(textureAsset));
			if (importer is UnityEditor.TextureImporter textureImporter)
				normalMap = textureImporter.textureType == UnityEditor.TextureImporterType.NormalMap;
#endif

			// Compressed normal maps need to be converted from AG to RG format
			if (normalMap)
			{
				if (postProcessedTexture == null)
					postProcessedTexture = new RenderTexture(1, 1, 0, GraphicsFormat.R16G16B16A16_SFloat);

				if (postProcessedTexture.width != textureAsset.width || postProcessedTexture.height != textureAsset.height)
				{
					postProcessedTexture.Release();
					postProcessedTexture.width = textureAsset.width;
					postProcessedTexture.height = textureAsset.height;
					postProcessedTexture.Create();
				}

				var blitMaterial = GetTempMaterial("Hidden/Mixture/TextureNode");
				MixtureUtils.SetTextureWithDimension(blitMaterial, "_Source", textureAsset);
				MixtureUtils.Blit(cmd, blitMaterial, textureAsset, postProcessedTexture);

				outputTexture = postProcessedTexture;
			}
			else
			{
				postProcessedTexture?.Release();
				outputTexture = textureAsset;
			}

			return true;
        }

		public bool InitializeNodeFromObject(Texture value)
		{
			textureAsset = value;
			return true;
		}
    }
}