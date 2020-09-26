using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Noises/Ridged Cellular Noise"), NodeMenuItem("Noises/Ridged Voronoi Noise")]
	public class RidgedCellularNoise : FixedNoiseNode
	{
		public override string name => "Ridged Cellular Noise";

		public override string shaderName => "Hidden/Mixture/RidgedCellularNoise";

		protected override IEnumerable<string> filteredOutProperties => base.filteredOutProperties.Concat(new string[]{ "_DistanceMode", "_CellsMode" });
	}
}