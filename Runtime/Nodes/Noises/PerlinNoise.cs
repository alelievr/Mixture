using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Noises/Perlin Noise")]
	public class PerlinNoise : FixedNoiseNode
	{
		public override string name => "Perlin Noise";

		public override string shaderName => "Hidden/Mixture/PerlinNoise";
	}
}