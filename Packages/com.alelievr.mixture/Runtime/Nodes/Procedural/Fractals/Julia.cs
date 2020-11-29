using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Procedural/Fractal/Julia")]
	public class Julia : FractalNode 
	{
		public override string name => "Julia";

		public override string shaderName => "Hidden/Mixture/Julia";

		// Enumerate the list of material properties that you don't want to be turned into a connectable port.
		protected override IEnumerable<string> filteredOutProperties => new string[]{"_Mode", "_Iteration"};
	}
}