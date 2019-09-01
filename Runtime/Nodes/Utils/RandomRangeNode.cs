using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Utils/Random Range")]
	public class RandomRangeNode : MixtureNode
	{
		[Output("Random")]
		public float	random;

		public float	min;
		public float	max;

		public override string name => "RandomRange";

		protected override bool ProcessNode()
		{
			random = Random.Range(min, max);
			return true;
		}
	}
}