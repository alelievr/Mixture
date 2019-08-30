using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Channels/Mask")]
	public class MaskNode : FixedShaderNode
	{
		public override string name => "Mask";

		public override string shaderName => "Hidden/Mixture/Mask";

		public override bool displayMaterialInspector => true;

        protected override IEnumerable<string> filteredOutProperties => new string[] { "_Mask" };
    }
}