using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Noises/Cellular Noise"), NodeMenuItem("Noises/Voronoi Noise")]
	public class CellularNoise : FixedNoiseNode
	{
		public override string name => "Cellular Noise";

		public override string shaderName => "Hidden/Mixture/CellularNoise";

		protected override IEnumerable<string> filteredOutProperties => base.filteredOutProperties.Concat(new string[]{ "_DistanceMode", "_CellsMode" });
	}
}