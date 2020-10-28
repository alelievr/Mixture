using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
	[Documentation(@"
Generate a circle, in 3D this node generate spheres.
")]

	[System.Serializable, NodeMenuItem("Procedural/Circles")]
	public class Circles : FixedShaderNode
	{
		public override string name => "Circles";

		public override string shaderName => "Hidden/Mixture/Circles";

		public override bool displayMaterialInspector => true;

		// Enumerate the list of material properties that you don't want to be turned into a connectable port.
		protected override IEnumerable<string> filteredOutProperties => new string[]{"_Tile", "_Offset"};
	}
}