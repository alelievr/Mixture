using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
	[Documentation(@"
Voronoi/Cellular Noise generator.
This node is useful to generate cloud like textures, organic cellular patterns or more exotic patterns with stars using the Minkowski distance mode.

Note that for Texture 2D, the z coordinate is used as a seed offset.
This allows you to generate multiple noises with the same UV.
Be careful with because if you use a UV with a distorted z value, you'll get a weird looking noise instead of the normal one.
")]

	[System.Serializable, NodeMenuItem("Noises/Cellular Noise"), NodeMenuItem("Noises/Voronoi Noise")]
	public class CellularNoise : FixedNoiseNode
	{
		public override string name => "Cellular Noise";

		public override string shaderName => "Hidden/Mixture/CellularNoise";

		protected override IEnumerable<string> filteredOutProperties => base.filteredOutProperties.Concat(new string[]{ "_DistanceMode", "_CellsMode" });
	}
}