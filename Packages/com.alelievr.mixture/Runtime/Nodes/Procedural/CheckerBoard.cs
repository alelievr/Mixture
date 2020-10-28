using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
	[Documentation(@"
Generate a checkerboard patter, in 3D this node generates a cubic checkerboard pattern.
")]

	[System.Serializable, NodeMenuItem("Procedural/CheckerBoard")]
	public class CheckerBoard : FixedShaderNode
	{
		public override string name => "CheckerBoard";

		public override string shaderName => "Hidden/Mixture/CheckerBoard";

		public override bool displayMaterialInspector => true;

		// Enumerate the list of material properties that you don't want to be turned into a connectable port.
		protected override IEnumerable<string> filteredOutProperties => new string[]{};
	}
}