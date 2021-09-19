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
Output a random color based on the HSV parameters.
")]

	[System.Serializable, NodeMenuItem("Math/Random Color")]
	public class RandomColorNode : MixtureNode, IRealtimeReset
	{
		public override bool hasSettings => false;

		[Output("Color")]
		public Color	r;

		[Input]
		public int 		seed;

		public float	minHue = 0.0f;
		public float	maxHue = 1.0f;
		public float	minSat = 0.0f;
		public float	maxSat = 1.0f;
		public float	minValue = 0.0f;
		public float	maxValue = 1.0f;

		public override string name => "Random Color";

		Random random;

		protected override void Enable()
			=> InitSeed();

		protected override bool ProcessNode(CommandBuffer cmd)
		{
			float hue = Mathf.Lerp(minHue, maxHue, (float)random.NextDouble());
			float sat = Mathf.Lerp(minSat, maxSat, (float)random.NextDouble());
			float val = Mathf.Lerp(minValue, maxValue, (float)random.NextDouble());
			r = Color.HSVToRGB(hue, sat, val);
			return true;
		}

		void InitSeed() => random = new Random(seed);

		public void RealtimeReset() => InitSeed();
	}
}