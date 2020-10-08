using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using UnityEngine.Rendering;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Color/EdgeDetect")]
	public class EdgeDetect : FixedShaderNode
	{
		public override string name => "EdgeDetect";

		public override string shaderName => "Hidden/Mixture/EdgeDetect";

		public override bool displayMaterialInspector => true;
	}
}