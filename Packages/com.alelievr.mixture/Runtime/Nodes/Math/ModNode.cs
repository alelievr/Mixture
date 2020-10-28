using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
	[Documentation(@"
Perform a modulo with `source A` and `source B`and writes the result to output like so:
```
_Output = _SourceA % _SourceB;
```
")]

	[System.Serializable, NodeMenuItem("Math/Mod")]
	public class ModNode : FixedShaderNode
	{
		public override string name => "Mod";

		public override string shaderName => "Hidden/Mixture/Mod";

		public override bool displayMaterialInspector => true;

		// Enumerate the list of material properties that you don't want to be turned into a connectable port.
		protected override IEnumerable<string> filteredOutProperties => new string[]{};
	}
}