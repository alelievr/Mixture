using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
	[Documentation(@"
Generate white noise.
")]

	[System.Serializable, NodeMenuItem("Noises/White Noise")]
	public class WhiteNoise : FixedShaderNode
	{
		public override string name => "White Noise";

		public override string shaderName => "Hidden/Mixture/WhiteNoise";

		public override bool displayMaterialInspector => true;

		// Enumerate the list of material properties that you don't want to be turned into a connectable port.
		protected override IEnumerable<string> filteredOutProperties => new string[]{};
	}
}