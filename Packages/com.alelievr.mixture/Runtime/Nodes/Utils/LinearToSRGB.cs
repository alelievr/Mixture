using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Utils/Linear To sRGB")]
	public class LinearToSRGB : FixedShaderNode
	{
		public override string name => "Linear To sRGB";

		public override string shaderName => "Hidden/Mixture/LinearToSRGB";

		public override bool displayMaterialInspector => true;

		// Enumerate the list of material properties that you don't want to be turned into a connectable port.
		protected override IEnumerable<string> filteredOutProperties => new string[]{};
	}
}