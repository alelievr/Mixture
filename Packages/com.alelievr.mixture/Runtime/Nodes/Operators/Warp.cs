using System.Collections.Generic;
using GraphProcessor;

namespace Mixture
{
	[Documentation(@"
Distort the input texture using a height map.
Internally this node converts the height map into a normal map and use it to distort the UVs to sample the input texture.
")]

	[System.Serializable, NodeMenuItem("Operators/Warp")]
	public class Warp : FixedShaderNode
	{
		public override string name => "Warp";

		public override string shaderName => "Hidden/Mixture/Warp";

		public override bool displayMaterialInspector => true;

		// Enumerate the list of material properties that you don't want to be turned into a connectable port.
		protected override IEnumerable<string> filteredOutProperties => new string[]{};

		protected override MixtureSettings defaultRTSettings => Get2DOnlyRTSettings(base.defaultRTSettings);

		public override List<OutputDimension> supportedDimensions => new List<OutputDimension>() {
			OutputDimension.Texture2D,
		};
	}
}