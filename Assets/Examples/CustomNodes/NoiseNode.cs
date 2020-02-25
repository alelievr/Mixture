using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Custom/NoiseNode")]
	public class NoiseNode : FixedShaderNode
	{
		public override string name => "NoiseNode";

		public override string shaderName => "Custom/NoiseNode";

		public override bool displayMaterialInspector => true;

		protected override IEnumerable<string> filteredOutProperties => new string[]{};
	}
}