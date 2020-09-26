using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Utils/Texture Sample")]
	public class TextureSampleNode : FixedShaderNode
	{
		public override string name => "Texture Sample";

		public override string shaderName => "Hidden/Mixture/TextureSample";

		public override bool displayMaterialInspector => true;
	}
}