using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
	[Documentation(@"
Generate a Disk, in 3D this node generate a solid spheres.
")]

	[System.Serializable, NodeMenuItem("Procedural/Disk")]
	public class Circles : FixedShaderNode
	{
		public override string name => "Disk";

		public override string shaderName => "Hidden/Mixture/Disk";

		public override bool displayMaterialInspector => true;

		// Enumerate the list of material properties that you don't want to be turned into a connectable port.
		protected override IEnumerable<string> filteredOutProperties => new string[]{"_Tile", "_Offset"};
	}
}