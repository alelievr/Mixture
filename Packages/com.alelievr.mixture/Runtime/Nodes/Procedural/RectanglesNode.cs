using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
	[Documentation(@"
Generate a rectangle pattern. In 3D this node generates cuboid shapes.
")]

	[System.Serializable, NodeMenuItem("Procedural/Rectangles")]
	public class RectanglesNode : FixedShaderNode
	{
		public override string name => "Rectangles";

		public override string shaderName => "Hidden/Mixture/Rectangles";

		public override bool displayMaterialInspector => true;

		// Enumerate the list of material properties that you don't want to be turned into a connectable port.
		protected override IEnumerable<string> filteredOutProperties => new string[]{};
	}
}