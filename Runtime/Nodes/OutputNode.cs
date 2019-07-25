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
		public CustomRenderTexture	tempRenderTexture;

		// Serialized properties for the view:
		public int				currentSlice;

		public event Action		onTempRenderTextureUpdated;

		public override string	name => "Output";
		public override Texture previewTexture => tempRenderTexture;
		
		Material				finalCopyMaterial;

		// Compression settings
		// TODO: there are too many formats, reduce them with a new enum
		public TextureFormat				compressionFormat = TextureFormat.RGBA32;
		public TextureCompressionQuality	compressionQuality = TextureCompressionQuality.Best;
		public bool							enableCompression = true;

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
			
			if (finalCopyMaterial == null)
				finalCopyMaterial = new Material(Shader.Find("Hidden/Mixture/FinalCopy"));

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

			MixtureUtils.SetupDimensionKeyword(finalCopyMaterial, input.dimension);
			if (input.dimension == TextureDimension.Tex2D)
				finalCopyMaterial.SetTexture("_Source2D", input);
			else if (input.dimension == TextureDimension.Tex3D)
				finalCopyMaterial.SetTexture("_Source3D", input);
			else
				finalCopyMaterial.SetTexture("_SourceCube", input);
				
			tempRenderTexture.material = finalCopyMaterial;
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