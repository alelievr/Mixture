using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
	[Documentation(@"
The Difference Node can be used to detect differences between two textures. You can choose between these modes:
- Error Diff, it performs an abs(A - B) operation between the two textures, you have a multiplier to help you detect faint details.
- Perceptual Diff, this one only works op color (no alpha) and will perform the difference operation in a perceptual color space (JzAzBz).
- Swap, let you swap between the two textures
- Onion Skin, let you interpolate between the two textures using a slider.
")]

	[System.Serializable, NodeMenuItem("Utils/Difference")]
	public class DifferenceNode : FixedShaderNode
	{
		public override string name => "Difference";

		public override string shaderName => "Hidden/Mixture/DifferenceNode";

		public override bool displayMaterialInspector => true;

		// Enumerate the list of material properties that you don't want to be turned into a connectable port.
		protected override IEnumerable<string> filteredOutProperties => new string[]{ "_Mode", "_ErrorMultiplier", "_Swap", "_Slide" };
	}
}