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
	public class RandomRangeNode : MixtureNode, IRealtimeReset
	{
		[Output("Random")]
		public Vector4	random;

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
			InitSeed();
		}

		protected override bool ProcessNode(CommandBuffer cmd)
		{
			random.x = Mathf.Lerp(min, max, (float)generator.NextDouble());
			random.y = Mathf.Lerp(min, max, (float)generator.NextDouble());
			random.z = Mathf.Lerp(min, max, (float)generator.NextDouble());
			random.w = Mathf.Lerp(min, max, (float)generator.NextDouble());
			return true;
		}

		void InitSeed() => generator = new Random(seed);

		public void RealtimeReset() => InitSeed();
	}
}