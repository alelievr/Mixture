using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
	[Documentation(@"
Generate a texture from an HDR color.
")]

	[System.Serializable, NodeMenuItem("Color/Uniform Color"), NodeMenuItem("Color/Color Matte")]
	public class ColorMatteNode : FixedShaderNode
	{
		public override string name => "Color Matte";

		public override string shaderName => "Hidden/Mixture/ColorMatte";

		public override bool displayMaterialInspector => true;
        public override float nodeWidth => MixtureUtils.smallNodeWidth; 

        public override bool hasPreview => false;
	}
}