﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Matte/Color Matte")]
	public class ColorMatteNode : FixedShaderNode
	{
		public override string name => "Color Matte";

		public override string shaderName => "Hidden/Mixture/ColorMatte";

		public override bool displayMaterialInspector => true;
	}
}