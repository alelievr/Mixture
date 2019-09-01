using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Operators/Vector Add")]
	public class VectorAddNode : MixtureNode
	{
		// TODO: multi Vectoradd port

		[Input("A")]
		public Vector4	a;
		
		[Input("B")]
		public Vector4	b;
		
		[Output("Out")]
		public Vector4	o;

		public override string name => "Add";

		protected override bool ProcessNode()
		{
			o = a + b;
			return false;
		}
	}
}