using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
	[Documentation(@"
Replace the source color by the target color in the image.
")]

	[System.Serializable, NodeMenuItem("Color/Swap Color"), NodeMenuItem("Colors/Replace Color")]
	public class ColorSwapNode : FixedShaderNode
	{
		public override string name => "Swap Color";

		public override string shaderName => "Hidden/Mixture/ColorSwap";

		public override bool displayMaterialInspector => true;

		protected override IEnumerable<string> filteredOutProperties => new string[]{ "_Mode" };
	}
}