using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
	[Documentation(@"
Clamp the input texture values. Note that the clamp is executed for each channel of the texture following this forumla:
```
_Output.rgba = clamp(_Input.rgba, _Min, _Max);
```
")]

	[System.Serializable, NodeMenuItem("Math/Clamp")]
	public class ClampNode : FixedShaderNode
	{
		public override string name => "Clamp";

		public override string shaderName => "Hidden/Mixture/Clamp";

		public override bool displayMaterialInspector => true;

		// Enumerate the list of material properties that you don't want to be turned into a connectable port.
		protected override IEnumerable<string> filteredOutProperties => new string[]{};
	}
}