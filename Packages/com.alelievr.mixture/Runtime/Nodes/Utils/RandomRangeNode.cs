using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using UnityEngine.Rendering;
using System;
using Random = System.Random;

namespace Mixture
{
	[Documentation(@"
Output a random float between the min and max values.
")]

	[System.Serializable, NodeMenuItem("Math/Random Range")]
	public class RandomRangeNode : MixtureNode
	{
		[Output("Random")]
		public float	random;

		[Input("Min"), SerializeField]
		public float	min = 0.0f;
		[Input("Max"), SerializeField]
		public float	max = 1.0f;
		[Input("Seed"), SerializeField]
		public int		seed;

		public override bool showDefaultInspector => true;
		public override string name => "Random Range";

		Random generator;

		protected override void Enable()
		{
            base.Enable();
			generator = new Random(seed);
		}

		protected override bool ProcessNode(CommandBuffer cmd)
		{
			random = Mathf.Lerp(min, max, (float)generator.NextDouble());
			return true;
		}
	}
}