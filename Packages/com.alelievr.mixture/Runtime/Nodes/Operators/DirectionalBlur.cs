using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Operators/Directional Blur")]
	public class DirectionalBlur : FixedShaderNode
	{
		public override string name => "DirectionalBlur";

		public override string shaderName => "Hidden/Mixture/DirectionalBlur";

		public override bool displayMaterialInspector => true;
	}
}