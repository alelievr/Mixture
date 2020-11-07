using GraphProcessor;

namespace Mixture
{
	[Documentation(@"
Perlin Noise generator.

Note that for Texture 2D, the z coordinate is used as a seed offset.
This allows you to generate multiple noises with the same UV.
Be careful with because if you use a UV with a distorted z value, you'll get a weird looking noise instead of the normal one.
")]

	[System.Serializable, NodeMenuItem("Noises/Perlin Noise")]
	public class PerlinNoise : FixedNoiseNode
	{
		public override string name => "Perlin Noise";

		public override string shaderName => "Hidden/Mixture/PerlinNoise";
	}
}