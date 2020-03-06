using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Utils/Sample Gradient")]
	public class SampleGradientNode : MixtureNode, INeedsCPU
	{
        [Input("x"),Range(0.0f,1.0f)]
        public float x=0.0f;

		[Output("Color")]
		public Color	color;

        public Gradient gradient = new Gradient();

        public override string name => "Sample Gradient";

		protected override bool ProcessNode()
		{
            color = gradient.Evaluate(x);
			return true;
		}
	}
}