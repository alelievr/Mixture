using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
	[Documentation(@"
Sample the target texture and mask it using input texture. Note that the mask is written in the alpha channel of the output.
")]

	[System.Serializable, NodeMenuItem("Channels/Mask")]
	public class MaskNode : FixedShaderNode
	{
		public override string name => "Mask";

		public override string shaderName => "Hidden/Mixture/Mask";

		public override bool displayMaterialInspector => true;

        protected override IEnumerable<string> filteredOutProperties => new string[] { "_Mask" };
    }
}