using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
	[Documentation(@"
Discretize an image, rounding it's color components to make the specified number of steps in the image.
This node can also be used to make a posterize effect.

By default the input values are considered to be between 0 and 1, you can change these values in the node inspector to adapt the effect to your input data.
")]

	[System.Serializable, NodeMenuItem("Operators/Discretize")]
	public class Discretize : FixedShaderNode
	{
		public override string name => "Discretize";

		public override string shaderName => "Hidden/Mixture/Discretize";

		public override bool displayMaterialInspector => true;

		// Enumerate the list of material properties that you don't want to be turned into a connectable port.
		protected override IEnumerable<string> filteredOutProperties => new string[]{};
	}
}