using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Utils/sRGB To Linear")]
	public class SRGBToLinear : FixedShaderNode
	{
		public override string name => "sRGB To Linear";

		public override string shaderName => "Hidden/Mixture/SRGBToLinear";

		public override bool displayMaterialInspector => true;

		// Enumerate the list of material properties that you don't want to be turned into a connectable port.
		protected override IEnumerable<string> filteredOutProperties => new string[]{};
	}
}