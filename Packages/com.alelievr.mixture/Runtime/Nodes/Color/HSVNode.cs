using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
	[Documentation(@"
Modify the image in the HSV color space.
")]

	[System.Serializable, NodeMenuItem("Color/Hue Saturation Value")]
	public class HSVNode : FixedShaderNode
	{
		public override string name => "Hue Saturation Value";

		public override string shaderName => "Hidden/Mixture/HSV";

		public override bool displayMaterialInspector => true;

    }
}