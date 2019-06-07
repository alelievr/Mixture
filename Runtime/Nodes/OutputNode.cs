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
	[System.Serializable]
	public class OutputNode : MixtureNode
	{
		[Input(name = "In")]
		public Texture			input;

		// TODO
		// [Input(name = "Target size")]
		// public Vector2		targetSize;

		[HideInInspector, SerializeField]
		public Vector3Int		targetSize = new Vector3Int(512, 512, 1);
		[HideInInspector, SerializeField]
		public GraphicsFormat	format = GraphicsFormat.R8G8B8A8_SRGB;
		public int				mipmapCount = 1;
		public FilterMode		filterMode;

		// We use a temporary renderTexture to display the result of the graph
		// in the preview so we don't have to readback the memory each time we change something
		[NonSerialized, HideInInspector]
		public RenderTexture	tempRenderTexture;

		// Output texture properties
		public int				sliceCount = 1;
		public TextureDimension	dimension = TextureDimension.Tex2D;

		public event Action		onTempRenderTextureUpdated;

		new MixtureGraph		graph;

		public override string	name => "Output";

		protected override void Enable()
		{
			graph = base.graph as MixtureGraph;

			UpdateTempRenderTexture(ref tempRenderTexture);
		}

		protected override void Process()
		{
			if (graph.outputTexture == null)
			{
				Debug.LogError("Output Node can't write to target texture, Graph references a null output texture");
				return ;
			}

			// Update the renderTexture size and format:
			if (UpdateTempRenderTexture(ref tempRenderTexture))
				onTempRenderTextureUpdated?.Invoke();

			if (input?.GetType() != graph.outputTexture.GetType())
			{
				Debug.LogError("Error: Expected texture type input for the OutputNode is " + graph.outputTexture.GetType() + " but " + input?.GetType() + " was provided");
				return ;
			}

			// TODO: instead of a blit, use a copytexture
			switch (graph.outputTexture)
			{
				case Texture2D t:
					Graphics.Blit(input, tempRenderTexture);
					break ;
				case Texture2DArray t:
					for (int sliceIndex = 0; sliceIndex < t.depth; sliceIndex++)
						Graphics.Blit(input, tempRenderTexture, sliceIndex, sliceIndex);
					break ;
				case Texture3D t:
					break ;
				default:
					Debug.LogError("Output Texture type " + graph.outputTexture + " is not supported");
					break ;
			}
		}
	}
}