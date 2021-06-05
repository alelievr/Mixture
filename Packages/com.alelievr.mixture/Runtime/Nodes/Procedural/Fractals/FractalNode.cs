using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Custom/FractalNode")]
	public abstract class FractalNode : FixedShaderNode
	{
		public override bool displayMaterialInspector => true;

		protected override MixtureSettings defaultSettings => Get2DOnlyRTSettings(base.defaultSettings);

		// Override this if you node is not compatible with all dimensions
		public override List<OutputDimension> supportedDimensions => new List<OutputDimension>() {
			OutputDimension.Texture2D,
		};
	}
}