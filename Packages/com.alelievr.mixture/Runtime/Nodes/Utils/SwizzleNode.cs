using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
	[Documentation(@"
Swizzle the channels of the input texture, you can also choose to put a custom value instead of reusing a channel.
")]

	[System.Serializable, NodeMenuItem("Channels/Swizzle")]
	public class SwizzleNode : FixedShaderNode
	{
		public override bool hasSettings => false;

		public override string name => "Swizzle";

		public override string shaderName => "Hidden/Mixture/Swizzle";

		public override bool displayMaterialInspector => true;

		protected override IEnumerable<string> filteredOutProperties => new string[]{"_RMode","_GMode","_BMode","_AMode"};
	}
}