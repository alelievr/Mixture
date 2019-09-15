using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Custom/Ridged Perlin Noise")]
	public class RidgedPerlinNoise : FixedNoiseNode
	{
		public override string name => "Ridged Perlin Noise";

		public override string shaderName => "Hidden/Mixture/RidgedPerlinNoise";
	}
}