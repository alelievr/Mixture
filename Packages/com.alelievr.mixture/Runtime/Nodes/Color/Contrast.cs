using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
	[Documentation(@"
Allow to adjust the contrast and luminosity of the image.
Internally, the node converts the color to HSL, modifies the component S and L and convert the color back to RGB.
")]

	[System.Serializable, NodeMenuItem("Color/Contrast"), NodeMenuItem("Color/Luminosity")]
	public class Contrast : FixedShaderNode
	{
		public override string name => "Contrast";

		public override string shaderName => "Hidden/Mixture/Contrast";

		public override bool displayMaterialInspector => true;

		// Enumerate the list of material properties that you don't want to be turned into a connectable port.
		protected override IEnumerable<string> filteredOutProperties => new string[]{};
	}
}