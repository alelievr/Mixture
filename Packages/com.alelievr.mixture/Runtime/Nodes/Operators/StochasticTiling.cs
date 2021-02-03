using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
	[Documentation(@"
Make a texture tile following the stochastic method described in this paper: https://drive.google.com/file/d/1QecekuuyWgw68HU9tg6ENfrCTCVIjm6l/view
")]

	[System.Serializable, NodeMenuItem("Operators/Stochastic Tiling")]
	public class StochasticTiling : FixedShaderNode
	{
		public override string name => "Stochastic Tiling";

		public override string shaderName => "Hidden/Mixture/StochasticTiling";

		public override bool displayMaterialInspector => true;

		// Enumerate the list of material properties that you don't want to be turned into a connectable port.
		protected override IEnumerable<string> filteredOutProperties => new string[]{};

		protected override MixtureRTSettings defaultRTSettings => Get2DOnlyRTSettings(base.defaultRTSettings);

		// Override this if you node is not compatible with all dimensions
		public override List<OutputDimension> supportedDimensions => new List<OutputDimension>() {
			OutputDimension.Texture2D,
		};
	}
}