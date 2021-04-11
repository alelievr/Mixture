using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
	[Documentation(@"
Mirror the input texture along an axis or a corner.
")]

	[System.Serializable, NodeMenuItem("Operators/Mirror")]
	public class Mirror : FixedShaderNode
	{
		public override string name => "Mirror";

		public override string shaderName => "Hidden/Mixture/Mirror";

		public override bool displayMaterialInspector => true;

		// Enumerate the list of material properties that you don't want to be turned into a connectable port.
		protected override IEnumerable<string> filteredOutProperties => new string[]{ "_Mode", "_CornerType", "_CornerZPosition" };
	}
}