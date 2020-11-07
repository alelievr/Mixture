using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
	[Documentation(@"
Constant UV. Note that for texture 2D, the z coordinate is set to 0.5.
")]

	[System.Serializable, NodeMenuItem("Constants/UV")]
	public class UVNode : FixedShaderNode
	{
		public override string name => "UV";

		public override string shaderName => "Hidden/Mixture/UV";

		public override bool displayMaterialInspector => true;

	}
}