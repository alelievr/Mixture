using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System;
using UnityEngine.Rendering;
using System.Linq;

namespace Mixture
{
	[System.Serializable]
	public class ProcessorInputNode : MixtureNode
	{
		[Output(name = "Out")]
		public Texture			output;

        [HideInInspector]
        public int              inputMip;

		// We use a temporary renderTexture to display the result of the graph
		// in the preview so we don't have to readback the memory each time we change something
		[NonSerialized, HideInInspector]
		public CustomRenderTexture	tempRenderTexture;

		// Serialized properties for the view:
		public int					currentSlice;

		// public event Action			onTempRenderTextureUpdated;

		public override string		name => "Input";
		public override Texture 	previewTexture => tempRenderTexture;
		public override float		nodeWidth => 320;
		
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

        protected override void Disable() => CoreUtils.Destroy(tempRenderTexture);

		[CustomPortBehavior(nameof(output))]
		protected IEnumerable< PortData > ChangeOutputPortType(List< SerializableEdge > edges)
		{
			yield return new PortData{
				displayName = "output",
				displayType = TextureUtils.GetTypeFromDimension((TextureDimension)rtSettings.dimension),
				identifier = "output",
			};
		}
	}
}