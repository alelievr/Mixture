using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Color/Black And White")]
	public class BlackAndWhiteNode : FixedShaderNode
	{
		public override string name => "Black And White";

		public override string shaderName => "Hidden/Mixture/BlackAndWhite";

		public override bool displayMaterialInspector => true;

    }
}