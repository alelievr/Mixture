using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
		[Documentation(@"
Just like the cellular noise node, this one generate a cellular pattern but the octaves are accumulated with an absolute function, which create these small ""ridges"" in the noise.

Note that for Texture 2D, the z coordinate is used as a seed offset.
This allows you to generate multiple noises with the same UV.
Be careful with because if you use a UV with a distorted z value, you'll get a weird looking noise instead of the normal one.
")]

	[System.Serializable, NodeMenuItem("Noises/Ridged Cellular Noise"), NodeMenuItem("Noises/Ridged Voronoi Noise")]
	public class RidgedCellularNoise : FixedNoiseNode
	{
		public override string name => "Ridged Cellular Noise";

		public override string shaderName => "Hidden/Mixture/RidgedCellularNoise";

		protected override IEnumerable<string> filteredOutProperties => base.filteredOutProperties.Concat(new string[]{ "_DistanceMode", "_CellsModeR", "_CellsModeG", "_CellsModeB", "_CellsModeA" });
	}
}