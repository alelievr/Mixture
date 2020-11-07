using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
	[Documentation(@"
Generate a noise using multiple octaves of perlin noise, combined using the Fractal Brownian motion algorithm.
")]

	[System.Serializable, NodeMenuItem("Noises/Fractal Brownian Motion")]
	public class FractalBrownianMotionNoise : FixedNoiseNode
	{
		public override string name => "Fractal Brownian Motion";

		public override string shaderName => "Hidden/Mixture/FractalBrownianMotionNoise";
	}
}