using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Operations/Lerp")]
	public class LerpNode : FixedShaderNode
	{
		public override string name => "Lerp";

		public override string shaderName => "Hidden/Mixture/Lerp";

		public override bool displayMaterialInspector => true;

	}
}