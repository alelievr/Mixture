using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Color/Threshold")]
	public class ThresholdNode : FixedShaderNode
	{
		public override string name => "Threshold";

		public override string shaderName => "Hidden/Mixture/Threshold";

		public override bool displayMaterialInspector => true;

        protected override IEnumerable<string> filteredOutProperties => new string[] { "_Channel" };
    }
}