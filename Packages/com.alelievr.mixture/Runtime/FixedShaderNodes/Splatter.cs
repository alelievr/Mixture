using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using UnityEngine.Rendering;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Textures/Splatter"), NodeMenuItem("Textures/Scatter")]
	public class Splatter : FixedShaderNode
	{
		public override string name => "Splatter";

		public override string shaderName => "Hidden/Mixture/Splatter";

		public override bool displayMaterialInspector => true;

        protected override IEnumerable<string> filteredOutProperties => new List<string>(){
			"_SourceCrop", "_Sequence", "_Size", "_Operator"
		};

		public override List<OutputDimension> supportedDimensions => new List<OutputDimension>() {
			// TODO: support of Texture3D
			OutputDimension.Texture2D,
		};
	}
}