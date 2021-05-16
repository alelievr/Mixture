using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
	[Documentation(@"
Multiply two normal maps using the surface gradient functions.
")]

	[System.Serializable, NodeMenuItem("Normal/Normal Mul")]
	public class NormalMul : FixedShaderNode
	{
		public override string name => "Normal Mul";

		public override string shaderName => "Hidden/Mixture/NormalMul";

		public override bool displayMaterialInspector => true;

		// Enumerate the list of material properties that you don't want to be turned into a connectable port.
		protected override IEnumerable<string> filteredOutProperties => new string[]{};

		protected override MixtureSettings defaultSettings => Get2DOnlyRTSettings(base.defaultSettings);

		// Override this if you node is not compatible with all dimensions
		public override List<OutputDimension> supportedDimensions => new List<OutputDimension>() {
			OutputDimension.Texture2D,
		};
	}
}