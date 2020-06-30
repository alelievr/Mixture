using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using UnityEngine.Rendering;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Utils/Random Range")]
	public class RandomRangeNode : MixtureNode
	{
		[Output("Random")]
		public float	random;

		public float	min = 0.0f;
		public float	max = 1.0f;

		public override string name => "Random Range";

		protected override bool ProcessNode(CommandBuffer cmd)
		{
			random = Random.Range(min, max);
			return true;
		}
	}
}