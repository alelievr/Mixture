using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
	[Documentation(@"
Constant Polar UV. Note that for texture 2D, the z coordinate is set to 0.5.
")]

	[System.Serializable, NodeMenuItem("Constants/UV Polar")]
	public class UVPolarNode : FixedShaderNode
	{
		public override string name => "UV Polar";

		public override string shaderName => "Hidden/Mixture/UVPolar";

		public override bool displayMaterialInspector => true;

		public override List<OutputDimension> supportedDimensions => new List<OutputDimension>() {
			OutputDimension.Texture2D,
			OutputDimension.Texture3D,
		};

	}
}