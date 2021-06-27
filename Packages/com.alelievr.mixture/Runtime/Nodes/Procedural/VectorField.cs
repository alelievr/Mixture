using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Procedural/Vector Field")]
	public class VectorField : FixedShaderNode
	{
		public override string name => "Vector Field";

		public override string shaderName => "Hidden/Mixture/VectorField";

		public override bool displayMaterialInspector => true;

		// Enumerate the list of material properties that you don't want to be turned into a connectable port.
		protected override IEnumerable<string> filteredOutProperties => new string[]{ "_Mode", "_Direction", "_PointInwards"};
	}
}