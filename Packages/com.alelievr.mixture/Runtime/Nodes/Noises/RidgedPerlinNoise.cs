using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
	[Documentation(@"
Just like the perlin noise node, this one generate a cloudy pattern but the octaves are accumulated with an absolute function, which create these small ""ridges"" in the noise.

Note that for Texture 2D, the z coordinate is used as a seed offset.
This allows you to generate multiple noises with the same UV.
Be careful with because if you use a UV with a distorted z value, you'll get a weird looking noise instead of the normal one.
")]

	[System.Serializable, NodeMenuItem("Noises/Ridged Perlin Noise")]
	public class RidgedPerlinNoise : FixedNoiseNode
	{
		public override string name => "Ridged Perlin Noise";

		public override string shaderName => "Hidden/Mixture/RidgedPerlinNoise";
	}
}