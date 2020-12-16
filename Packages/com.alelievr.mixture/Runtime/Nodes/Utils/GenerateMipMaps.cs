using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using UnityEngine.Rendering;
using System;

namespace Mixture
{
	[Documentation(@"
Generate mipmaps for the input texture.
")]

	[System.Serializable, NodeMenuItem("Utils/Generate MipMaps")]
	public class GenerateMipMaps : MixtureNode 
	{
		[Input(name = "In")]
		public Texture	input;

		[Output(name = "Out"), Tooltip("Output Texture")]
		public CustomRenderTexture	output = null;

		public override string	name => "Generate MipMaps";

        protected virtual IEnumerable<string> filteredOutProperties => Enumerable.Empty<string>();
		public override Texture previewTexture => output;

		protected override void Enable()
		{
			UpdateTempRenderTexture(ref output, hasMips: true);
		}

        protected override void Disable()
		{
			base.Disable();
			CoreUtils.Destroy(output);
		}

		protected override bool ProcessNode(CommandBuffer cmd)
		{
			if (!base.ProcessNode(cmd))
				return false;

			if (input == null)
				return false;

			for (int i = 0; i < TextureUtils.GetSliceCount(input); i++)
				cmd.CopyTexture(input, i, 0, output, i, 0);
			cmd.GenerateMips(output);

			return true;
		}
    }
}