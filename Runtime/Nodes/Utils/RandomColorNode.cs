using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Utils/Random Color")]
	public class RandomColorNode : MixtureNode
	{
		[Input("Color")]
		public Color	r;

		public float	minHue;
		public float	maxHue;
		public float	minSat;
		public float	maxSat;
		public float	minValue;
		public float	maxValue;

		public override string name => "RandomColor";

		protected override bool ProcessNode()
		{
			r = Random.ColorHSV(minHue, maxHue, minSat, maxSat, minValue, maxValue);
			return true;
		}
	}
}