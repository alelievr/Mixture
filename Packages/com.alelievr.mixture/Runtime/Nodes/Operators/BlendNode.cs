using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using UnityEngine.Rendering;

namespace Mixture
{
	[Documentation(@"
Blend between two textures, you can use different blend mode depending which texture you want to blend (depth, color, ect.).

You also have the possibility to provide a mask texture that will affect the opacity of the blend depending on the mask value.
The Mask Mode property is used to select which channel you want the mask value to use for the blending operation.

Note that for normal blending, please use the Normal Blend node.
")]

	[System.Serializable, NodeMenuItem("Color/Blend")]
	public class BlendNode : FixedShaderNode
	{
		public override string name => "Blend";

		public override string shaderName => "Hidden/Mixture/Blend";

		public override bool displayMaterialInspector => true;

		protected override IEnumerable<string> filteredOutProperties => new string[]{"_BlendMode", "_MaskMode", "_Opacity"};

		protected override bool ProcessNode(CommandBuffer cmd)
		{
			bool r = base.ProcessNode(cmd);

			return r;
		}
	}
}