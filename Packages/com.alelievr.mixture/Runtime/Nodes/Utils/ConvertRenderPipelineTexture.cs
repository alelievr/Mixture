using UnityEngine;
using GraphProcessor;
using UnityEngine.Rendering;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Utils/Render Texture From Pipeline")]
	public class ConvertRenderPipelineTexture : MixtureNode
	{
        [Input]
        public RenderPipelineTexture input;

        [Output]
        public RenderTexture output;

		public override string	name => "RenderPipelineTexture";

		public override bool showDefaultInspector => true;
        public override bool hasPreview => false;

		protected override bool ProcessNode(CommandBuffer cmd)
		{
			output = input.renderPipelineTexture;

			if (output != null)
			{
				settings.sizeMode = OutputSizeMode.Absolute;
				settings.width = output.width;
				settings.height = output.height;
				settings.depth = TextureUtils.GetSliceCount(output);
			}

			return output != null;
		}
    }
}