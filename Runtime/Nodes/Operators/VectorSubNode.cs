using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Operators/Vector Sub")]
	public class VectorSubNode : MixtureNode
	{
		// TODO: multi VectorSub port

		[Input("A")]
		public Vector4	a;
		
		[Input("B")]
		public Vector4	b;
		
		[Output("Out")]
		public Vector4	o;

		public override string name => "Sub";

		protected override bool ProcessNode()
		{
			o = a - b;
			return false;
		}
	}
}