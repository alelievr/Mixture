using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
	[Documentation(@"
Allow you to invert an image in the HSV color space.
")]

	[System.Serializable, NodeMenuItem("Color/Invert")]
	public class InvertNode : FixedShaderNode
	{
		public override string name => "Invert";

		public override string shaderName => "Hidden/Mixture/Invert";

		public override bool displayMaterialInspector => true;

        protected override IEnumerable<string> filteredOutProperties => new string[] { "_Hue", "_Saturation", "_Value", };
    }
}