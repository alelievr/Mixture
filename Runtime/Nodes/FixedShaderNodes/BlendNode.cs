﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Color/Blend")]
	public class BlendNode : FixedShaderNode
	{
		public override string name => "Blend";

		public override string shaderName => "Hidden/Mixture/Blend";

		public override bool displayMaterialInspector => true;

		public override Precision precision => Precision.Float;

		protected override IEnumerable<string> filteredOutProperties => new string[]{"_BlendMode", "_MaskMode"};

	}
}