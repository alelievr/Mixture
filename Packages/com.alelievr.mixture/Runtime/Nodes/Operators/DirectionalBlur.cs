using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
	[Documentation(@"
Blur the input texture using a Gaussian filter in the specified direction.

Note that the kernbel uses a fixed number of 32 samples, for high blur radius you may need to use two directional blur nodes.
")]

	[System.Serializable, NodeMenuItem("Operators/Directional Blur")]
	public class DirectionalBlur : FixedShaderNode
	{
		public override string name => "DirectionalBlur";

		public override string shaderName => "Hidden/Mixture/DirectionalBlur";

		public override bool displayMaterialInspector => true;
	}
}