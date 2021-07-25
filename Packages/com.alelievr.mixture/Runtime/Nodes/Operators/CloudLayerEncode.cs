using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
		[Documentation(@"
Encodes a Cubemap texture into a 2D map, the output texture is formated for the HDRP cloud layer system (latlong).
")]

	[System.Serializable, NodeMenuItem("Operators/Cloud Layer Encode")]
	public class CloudLayerEncode : FixedShaderNode
	{
		public override string name => "Cloud Layer Encode";

		public override string shaderName => "Hidden/Mixture/CloudLayerEncode";

		public override bool displayMaterialInspector => true;

		protected override MixtureSettings defaultSettings => Get2DOnlyRTSettings(base.defaultSettings);

		protected override IEnumerable<string> filteredOutProperties => new string[]{ "_UpperHemisphereOnly" };

		// Override this if you node is not compatible with all dimensions
		public override List<OutputDimension> supportedDimensions => new List<OutputDimension>() {
			OutputDimension.Texture2D,
		};
	}
}