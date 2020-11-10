using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
	[Documentation(@"
Add two normal maps using the surface gradient functions.
")]

	[System.Serializable, NodeMenuItem("Normal/Normal Add")]
	public class NormalAdd : FixedShaderNode
	{
		public override string name => "Normal Add";

		public override string shaderName => "Hidden/Mixture/NormalAdd";

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