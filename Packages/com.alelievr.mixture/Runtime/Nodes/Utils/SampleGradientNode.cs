using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using UnityEngine.Rendering;

namespace Mixture
{
	[Documentation(@"
Sample a gradient using a float value between 0 and 1.
")]

	[System.Serializable, NodeMenuItem("Utils/Sample Gradient")]
	public class SampleGradientNode : MixtureNode
	{
        [Input("x"),Range(0.0f,1.0f)]
        public float x=0.0f;

		[Output("Color")]
		public Color	color;

        public Gradient gradient = new Gradient();

        public override string name => "Sample Gradient";

		protected override bool ProcessNode(CommandBuffer cmd)
		{
            color = gradient.Evaluate(x);
			return true;
		}
	}
}