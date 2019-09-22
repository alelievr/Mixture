using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Noises/Cellular Noise")]
	public class CellularNoise : FixedNoiseNode
	{
		public override string name => "Cellular Noise";

		public override string shaderName => "Hidden/Mixture/CellularNoise";
	}
}