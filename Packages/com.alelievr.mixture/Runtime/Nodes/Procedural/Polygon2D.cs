using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Procedural/Polygon 2D")]
	public class Polygon2D : FixedShaderNode
	{
		public override string name => "Polygon 2D";

		public override string shaderName => "Hidden/Mixture/Polygon2D";

		public override bool displayMaterialInspector => true;

		protected override IEnumerable<string> filteredOutProperties => new string[]{ "_Starryness", "_Mode" };

		protected override MixtureSettings defaultRTSettings => Get2DOnlyRTSettings(base.defaultRTSettings);

		// Override this if you node is not compatible with all dimensions
		public override List<OutputDimension> supportedDimensions => new List<OutputDimension>() {
			OutputDimension.Texture2D,
		};
	}
}