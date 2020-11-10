using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
	[Documentation(@"
Generate a Normal map from an Height map. This node uses the surface gradient technique to perform this operation.
")]

	[System.Serializable, NodeMenuItem("Normal/Normal From Height"), NodeMenuItem("Normal/Height To Normal")]
	public class NormalFromHeight : FixedShaderNode
	{
		public override string name => "Normal From Height";

		public override string shaderName => "Hidden/Mixture/NormalFromHeight";

		public override bool displayMaterialInspector => true;

		// Enumerate the list of material properties that you don't want to be turned into a connectable port.
		protected override IEnumerable<string> filteredOutProperties => new string[]{};

		protected override MixtureRTSettings defaultRTSettings => Get2DOnlyRTSettings(base.defaultRTSettings);

		public override List<OutputDimension> supportedDimensions => new List<OutputDimension>() {
			OutputDimension.Texture2D,
		};
	}
}