using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
	[Documentation(@"
Apply a threshold value to a channel of the input texture and output the result. You can use the Feather parameter to smooth the step.
")]

	[System.Serializable, NodeMenuItem("Color/Threshold")]
	public class ThresholdNode : FixedShaderNode
	{
		public override string name => "Threshold";

		public override string shaderName => "Hidden/Mixture/Threshold";

		public override bool displayMaterialInspector => true;

        protected override IEnumerable<string> filteredOutProperties => new string[] { "_Channel" };
    }
}