using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Procedural/Fractal/Menger Sponge")]
	public class MengerSponge : FractalNode
	{
		public override string name => "Menger Sponge";

		public override string shaderName => "Hidden/Mixture/MengerSponge";

		// Enumerate the list of material properties that you don't want to be turned into a connectable port.
		protected override IEnumerable<string> filteredOutProperties => new string[]{"_Mode"};
	}
}