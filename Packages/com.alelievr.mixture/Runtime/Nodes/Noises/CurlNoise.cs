using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
    	[Documentation(@"
Curl noise is similar math to the Perlin Noise, but with the addition of a curl function which allows it to generate a turbulent noise.
This resulting noise is incompressible (divergence-free), which means that the genearted vectors cannot converge to sink points.

The output of this node is a 2D or 3D vector field (normalized vector direction).
")]

	[System.Serializable, NodeMenuItem("Noises/Curl Noise")]
	public class CurlNoise : FixedNoiseNode 
	{
		public override string shaderName => "Hidden/Mixture/CurlNoise";

		public override string name => "Curl Noise";
	}
}