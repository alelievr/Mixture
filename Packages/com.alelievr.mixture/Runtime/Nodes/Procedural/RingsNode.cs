using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
	[Documentation(@"
Generates a ring pattern. In 3D this node generate toruses.
")]

	[System.Serializable, NodeMenuItem("Procedural/Rings"), NodeMenuItem("Procedural/Torus")]
	public class RingsNode : FixedShaderNode
	{
		public override string name => "Rings";

		public override string shaderName => "Hidden/Mixture/Rings";

		public override bool displayMaterialInspector => true;

		// Enumerate the list of material properties that you don't want to be turned into a connectable port.
		protected override IEnumerable<string> filteredOutProperties => new string[]{"_Scale", "_Offset"};
	}
}