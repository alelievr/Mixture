using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
	[Documentation(@"
Decodes a 2D texture into a cubemap, the input texture has to be formated for the HDRP cloud layer system (latlong).
")]

	[System.Serializable, NodeMenuItem("Operators/Cloud Layer Decode")]
	public class CloudLayerDecode : FixedShaderNode
	{
		public override string name => "Cloud Layer Decode";

		public override string shaderName => "Hidden/Mixture/CloudLayerDecode";

		public override bool displayMaterialInspector => true;

		protected override MixtureSettings defaultSettings => GetCubeOnlyRTSettings(base.defaultSettings);

		protected override IEnumerable<string> filteredOutProperties => new string[]{ "_UpperHemisphereOnly" };

		// Override this if you node is not compatible with all dimensions
		public override List<OutputDimension> supportedDimensions => new List<OutputDimension>() {
			OutputDimension.CubeMap,
		};
	}
}