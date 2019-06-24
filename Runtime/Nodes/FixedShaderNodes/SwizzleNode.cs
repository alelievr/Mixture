using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Channels/Swizzle")]
	public class SwizzleNode : FixedShaderNode
	{
		public override string name => "Swizzle";

		public override string shaderName => "Hidden/Mixture/Swizzle";

		public override bool displayMaterialInspector => true;
	}
}