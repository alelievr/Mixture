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

		public int				mipmapCount = 1;

		// We use a temporary renderTexture to display the result of the graph
		// in the preview so we don't have to readback the memory each time we change something
		[NonSerialized, HideInInspector]
		public RenderTexture	tempRenderTexture;

		// Serialized properties for the view:
		public int				currentSlice;

		public event Action		onTempRenderTextureUpdated;

		public override string	name => "Output";

		public override Texture previewTexture {get { return tempRenderTexture; } }

		public override MixtureRTSettings defaultRTSettings
		{
			get 
			{
				return new MixtureRTSettings()
            	{
					widthMode = OutputSizeMode.Fixed,
					heightMode = OutputSizeMode.Fixed,
					depthMode = OutputSizeMode.Fixed,
					width = 512,
					height = 512,
					sliceCount = 1,
					editFlags = EditFlags.Width | EditFlags.Height | EditFlags.Depth | EditFlags.Dimension | EditFlags.TargetFormat
				};
			}
		}
		
		protected override void Enable()
		{
			// Sanitize the RT Settings for the output node, they must contains only valid information for the output node
			if (rtSettings.targetFormat == OutputFormat.Default)
				rtSettings.targetFormat = OutputFormat.RGBA_Float;
			if (rtSettings.dimension == OutputDimension.Default)
				rtSettings.dimension = OutputDimension.Texture2D;

            UpdateTempRenderTexture(ref tempRenderTexture);
			graph.onOutputTextureUpdated += () => {
				UpdateTempRenderTexture(ref tempRenderTexture);
			};

			onSettingsChanged += () => {
				graph.UpdateOutputTexture();
			};
		}

		protected override void Process()
		{
			if (graph.outputTexture == null)
			{
				Debug.LogError("Output Node can't write to target texture, Graph references a null output texture");
				return ;
			}

			var inputPort = GetPort(nameof(input), nameof(input));

			if (inputPort.GetEdges().Count == 0)
			{
				Debug.LogWarning("Output node input is not connected");
				input = TextureUtils.GetBlackTexture(rtSettings);
				// TODO: set a black texture of texture dimension as default value
				return;
			}

			// Update the renderTexture size and format:
			if (UpdateTempRenderTexture(ref tempRenderTexture))
				onTempRenderTextureUpdated?.Invoke();

			if (input.dimension != graph.outputTexture.dimension)
			{
				Debug.LogError("Error: Expected texture type input for the OutputNode is " + graph.outputTexture.dimension + " but " + input?.dimension + " was provided");
				return ;
			}

			// TODO: instead of a blit, use a copytexture
			switch (graph.outputTexture)
			{
				case Texture2D t:
					Graphics.Blit(input, tempRenderTexture, new Vector2(1,-1), new Vector2(0,1));
					break ;
				case Texture2DArray t:
					for (int sliceIndex = 0; sliceIndex < t.depth; sliceIndex++)
						Graphics.Blit(input, tempRenderTexture, new Vector2(1,-1), new Vector2(0,1), sliceIndex, sliceIndex );
					break ;
				case Texture3D t:
					break ;
				default:
					Debug.LogError("Output Texture type " + graph.outputTexture + " is not supported");
					break ;
			}
		}

		[CustomPortBehavior(nameof(input))]
		protected IEnumerable< PortData > ChangeOutputPortType(List< SerializableEdge > edges)
		{
			yield return new PortData{
				displayName = "input",
				displayType = TextureUtils.GetTypeFromDimension((TextureDimension)rtSettings.dimension),
				identifier = "input",
			};
		}
	}
}