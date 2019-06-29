using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Coordinates/UV")]
	public class UVNode : FixedShaderNode
	{
		public override string name => "UV";

		public override string shaderName => "Hidden/Mixture/UV";

		public override bool displayMaterialInspector => true;

	}
}