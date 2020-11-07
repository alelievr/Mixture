using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
		[Documentation(@"
Perform a multiplication with `source A`, `source B` and Vector and writes the result to output like so:
```
_Output = _SourceA * _SourceB * _Value;
```
")]


	[System.Serializable, NodeMenuItem("Math/Mul")]
	public class MulNode : FixedShaderNode
	{
		public override string name => "Mul";

		public override string shaderName => "Hidden/Mixture/Mul";

		public override bool displayMaterialInspector => true;

		// Enumerate the list of material properties that you don't want to be turned into a connectable port.
		protected override IEnumerable<string> filteredOutProperties => new string[]{};
	}
}