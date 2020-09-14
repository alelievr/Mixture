using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using UnityEngine.Rendering;

namespace Mixture
{
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