using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Operators/Combine")]
	public class CombineNode : FixedShaderNode
	{
		public override string name => "CombineNode";

		public override string shaderName => "Hidden/Mixture/Combine";

		public override bool displayMaterialInspector => true;

		// Enumerate the list of material properties that you don't want to be turned into a connectable port.
		protected override IEnumerable<string> filteredOutProperties => new string[]{"_CombineModeR", "_CombineModeG", "_CombineModeB", "_CombineModeA"};
	}
}