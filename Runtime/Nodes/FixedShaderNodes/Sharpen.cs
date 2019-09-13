using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using UnityEngine.Rendering;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Color/Sharpen")]
	public class Sharpen : FixedShaderNode
	{
		public override string name => "Sharpen";

		public override string shaderName => "Hidden/Mixture/Sharpen";

		public override bool displayMaterialInspector => true;
	}
}