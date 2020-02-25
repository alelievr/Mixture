using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Matte/Gradient Matte")]
	public class GradienMattetNode : FixedShaderNode
	{
		public override string name => "Gradient Matte";

		public override string shaderName => "Hidden/Mixture/GradientMatte";

		public override bool displayMaterialInspector => true;

		protected override IEnumerable<string> filteredOutProperties => new string[]{"_Mode"};

	}
}