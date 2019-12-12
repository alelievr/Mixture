using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System;
using UnityEngine.Rendering;

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
		public int					currentSlice;

		public event Action			onTempRenderTextureUpdated;

		public override string		name => "Output";
		public override Texture 	previewTexture => graph.isRealtime ? graph.outputTexture : tempRenderTexture;
		public override float		nodeWidth => 320;

		Material					_finalCopyMaterial;
		Material					finalCopyMaterial
		{
			get
			{
				if (_finalCopyMaterial == null)
					_finalCopyMaterial = new Material(Shader.Find("Hidden/Mixture/FinalCopy"));
				return _finalCopyMaterial;
			}
		}

		// Compression settings
		// TODO: there are too many formats, reduce them with a new enum
		public MixtureCompressionFormat		compressionFormat = MixtureCompressionFormat.DXT5Crunched;
		public MixtureCompressionQuality	compressionQuality = MixtureCompressionQuality.Best;
		public bool							enableCompression = false;
		
		// TODO: move this to NodeGraphProcessor
		[NonSerialized]
		protected HashSet< string > uniqueMessages = new HashSet< string >();

		protected override MixtureRTSettings defaultRTSettings
        {
            get => new MixtureRTSettings()
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

        protected override void Enable()
        {
			// Sanitize the RT Settings for the output node, they must contains only valid information for the output node
			if (rtSettings.targetFormat == OutputFormat.Default)
				rtSettings.targetFormat = OutputFormat.RGBA_Float;
			if (rtSettings.dimension == OutputDimension.Default)
				rtSettings.dimension = OutputDimension.Texture2D;

			if (graph.isRealtime)
			{
				tempRenderTexture = graph.outputTexture as CustomRenderTexture;
			}
			else
			{
				UpdateTempRenderTexture(ref tempRenderTexture);
				graph.onOutputTextureUpdated += () => {
					UpdateTempRenderTexture(ref tempRenderTexture);
				};
			}

			onSettingsChanged += () => {
				graph.UpdateOutputTexture();
			};
		}

		protected override bool ProcessNode()
		{
			if (graph.outputTexture == null)
			{
				Debug.LogError("Output Node can't write to target texture, Graph references a null output texture");
				return false;
			}
			
			// Update the renderTexture reference for realtime graph
			if (graph.isRealtime)
			{
				if (tempRenderTexture != graph.outputTexture)
					onTempRenderTextureUpdated?.Invoke();
				tempRenderTexture = graph.outputTexture as CustomRenderTexture;
			}

			var inputPort = GetPort(nameof(input), nameof(input));

			if (inputPort.GetEdges().Count == 0)
			{
				if (uniqueMessages.Add("OutputNotConnected"))
					AddMessage("Output node input is not connected", NodeMessageType.Warning);
			}
			else
			{
				uniqueMessages.Clear();
				ClearMessages();
			}

			// Update the renderTexture size and format:
			if (UpdateTempRenderTexture(ref tempRenderTexture))
				onTempRenderTextureUpdated?.Invoke();
				
			// Manually reset all texture inputs
			ResetMaterialPropertyToDefault(finalCopyMaterial, "_Source_2D");
			ResetMaterialPropertyToDefault(finalCopyMaterial, "_Source_3D");
			ResetMaterialPropertyToDefault(finalCopyMaterial, "_Source_Cube");

			if (input != null)
			{
				if ( input.dimension != graph.outputTexture.dimension)
				{
					Debug.LogError("Error: Expected texture type input for the OutputNode is " + graph.outputTexture.dimension + " but " + input?.dimension + " was provided");
					return false;
				}

				MixtureUtils.SetupDimensionKeyword(finalCopyMaterial, input.dimension);

				if (input.dimension == TextureDimension.Tex2D)
					finalCopyMaterial.SetTexture("_Source_2D", input);
				else if (input.dimension == TextureDimension.Tex3D)
					finalCopyMaterial.SetTexture("_Source_3D", input);
				else
					finalCopyMaterial.SetTexture("_Source_Cube", input);
			}

			tempRenderTexture.material = finalCopyMaterial;

			return true;
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