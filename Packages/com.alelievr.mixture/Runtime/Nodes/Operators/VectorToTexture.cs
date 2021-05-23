using UnityEngine;
using GraphProcessor;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Operators/Vector To Texture")]
	public class VectorToTexture : MixtureNode
	{
        [Input, ShowAsDrawer]
        public Vector4 input;

        [Output]
        public RenderTexture output;

		public override string	name => "Vector To Texture";
		public override bool showDefaultInspector => true;
		public override Texture previewTexture => null;

		protected override bool ProcessNode(CommandBuffer cmd)
		{
            if (!base.ProcessNode(cmd))
                return false;

			if (output == null)
				output = new RenderTexture(1, 1, 0, GraphicsFormat.R16G16B16A16_SFloat, 0);
			
			cmd.SetRenderTarget(output);
			cmd.ClearRenderTarget(false, true, (Color)input, 0);

			return true;
		}

        protected override void Disable()
		{
			output?.Release();
			base.Disable();
		}
    }
}