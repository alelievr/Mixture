using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using UnityEngine.Rendering;

namespace Mixture
{
	[Documentation(@"
Output a random color based on the HSV parameters.
")]

	[System.Serializable, NodeMenuItem("Utils/Random Color")]
	public class RandomColorNode : MixtureNode
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

		protected override bool ProcessNode(CommandBuffer cmd)
		{
			var oldState = Random.state;
			Random.InitState(seed);
			r = Random.ColorHSV(minHue, maxHue, minSat, maxSat, minValue, maxValue);
			Random.state = oldState;
			return true;
		}
	}
}