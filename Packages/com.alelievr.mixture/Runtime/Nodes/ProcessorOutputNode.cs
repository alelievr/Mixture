#if false
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System;
using UnityEngine.Rendering;
using System.Linq;

namespace Mixture
{
	public enum TargetMip
	{
		SameAsInput,
		InputMinusOne,
		InputPlusOne,
	}

	[System.Serializable]
	public class ProcessorOutputNode : MixtureNode
	{
		[Input(name = "In")]
		public Texture			input;

		public TargetMip		targetMip = TargetMip.InputMinusOne;

		// We use a temporary renderTexture to display the result of the graph
		// in the preview so we don't have to readback the memory each time we change something
		[NonSerialized, HideInInspector]
		public CustomRenderTexture	tempRenderTexture;

		// Serialized properties for the view:
		public int					currentSlice;

		public event Action			onTempRenderTextureUpdated;

		public override string		name => "Output";
		public override Texture 	previewTexture => tempRenderTexture;
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
			graph.nodes.FirstOrDefault(n => n is ProcessorInputNode);

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

        protected override void Disable() => CoreUtils.Destroy(tempRenderTexture);

		protected override bool ProcessNode(CommandBuffer cmd)
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
				input = TextureUtils.GetBlackTexture(rtSettings);
				// TODO: set a black texture of texture dimension as default value
				return false;
			}
			else
			{
				uniqueMessages.Clear();
				ClearMessages();
			}

			// Update the renderTexture size and format:
			if (UpdateTempRenderTexture(ref tempRenderTexture))
				onTempRenderTextureUpdated?.Invoke();

			if (input.dimension != graph.outputTexture.dimension)
			{
				Debug.LogError("Error: Expected texture type input for the OutputNode is " + graph.outputTexture.dimension + " but " + input?.dimension + " was provided");
				return false;
			}

			MixtureUtils.SetupDimensionKeyword(finalCopyMaterial, input.dimension);

			// Manually reset all texture inputs
			ResetMaterialPropertyToDefault(finalCopyMaterial, "_Source_2D");
			ResetMaterialPropertyToDefault(finalCopyMaterial, "_Source_3D");
			ResetMaterialPropertyToDefault(finalCopyMaterial, "_Source_Cube");

			if (input.dimension == TextureDimension.Tex2D)
				finalCopyMaterial.SetTexture("_Source_2D", input);
			else if (input.dimension == TextureDimension.Tex3D)
				finalCopyMaterial.SetTexture("_Source_3D", input);
			else
				finalCopyMaterial.SetTexture("_Source_Cube", input);

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
#endif