using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
	[Documentation(@"
Rotate the normal map vector with a certain angle in degree.
")]

	[System.Serializable, NodeMenuItem("Normal/Normal Rotation")]
	public class NormalRotate : FixedShaderNode
	{
		public override string name => "Normal Rotate";

		public override string shaderName => "Hidden/Mixture/NormalRotate";

		public override bool displayMaterialInspector => true;

		// Enumerate the list of material properties that you don't want to be turned into a connectable port.
		protected override IEnumerable<string> filteredOutProperties => new string[]{};

		protected override MixtureSettings defaultRTSettings => Get2DOnlyRTSettings(base.defaultRTSettings);

		// Override this if you node is not compatible with all dimensions
		public override List<OutputDimension> supportedDimensions => new List<OutputDimension>() {
			OutputDimension.Texture2D,
			OutputDimension.Texture3D,
			OutputDimension.CubeMap,
		};
	}
}