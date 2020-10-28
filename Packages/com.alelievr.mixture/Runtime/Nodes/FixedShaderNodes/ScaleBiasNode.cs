using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
	[Documentation(@"
Apply a Scale and Bias on  the input texture.
")]

	[System.Serializable, NodeMenuItem("Color/Scale & Bias")]
	public class ScaleBiasNode : FixedShaderNode
	{
		public override string name => "Scale & Bias";

		public override string shaderName => "Hidden/Mixture/ScaleBias";

		public override bool displayMaterialInspector => true;

		protected override IEnumerable<string> filteredOutProperties => new string[]{"_Mode"};

	}
}