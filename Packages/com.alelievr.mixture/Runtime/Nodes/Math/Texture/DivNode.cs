using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
	[Documentation(@"
Divide one texture by another plus a constant value. The result is computed like this:
```
_Output = _SourceA / _SourceB / _Value
```
")]

	[System.Serializable, NodeMenuItem("Math/Div")]
	public class DivNode : FixedShaderNode
	{
		public override string name => "Div";

		public override string shaderName => "Hidden/Mixture/Div";

		public override bool displayMaterialInspector => true;

		// Enumerate the list of material properties that you don't want to be turned into a connectable port.
		protected override IEnumerable<string> filteredOutProperties => new string[]{};
	}
}