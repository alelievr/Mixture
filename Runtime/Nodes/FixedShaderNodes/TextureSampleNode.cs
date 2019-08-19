using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Sample/Texture Sample")]
	public class TextureSampleNode : FixedShaderNode
	{
		public override string name => "Texture Sample";

		public override string shaderName => "Hidden/Mixture/TextureSample";

		public override bool displayMaterialInspector => true;

		// TODO: remove me (test only)
		public override List<OutputDimension> supportedDimensions => new List<OutputDimension>() {
			OutputDimension.Texture2D,
		};
	}
}