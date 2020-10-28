using System.Collections.Generic;
using GraphProcessor;
namespace Mixture
{
	[Documentation(@"
Generates a line pattern. In 3D this node generate cylinders using a signed distance field function.
")]

	[System.Serializable, NodeMenuItem("Procedural/Lines")]
	public class Lines : FixedShaderNode
	{
		public override string name => "Lines";

		public override string shaderName => "Hidden/Mixture/Lines";

		public override bool displayMaterialInspector => true;

		// Enumerate the list of material properties that you don't want to be turned into a connectable port.
		protected override IEnumerable<string> filteredOutProperties => new string[]{"_Tile", "_Offset", "_Rotation"};
	}
}