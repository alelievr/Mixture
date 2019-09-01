using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Operators/Vector Div")]
	public class VectorDivNode : MixtureNode
	{
		// TODO: multi VectorDiv port

		[Input("A")]
		public Vector4	a;
		
		[Input("B")]
		public Vector4	b;
		
		[Output("Out")]
		public Vector4	o;

		public override string name => "Div";

		protected override bool ProcessNode()
		{
			o = new Vector4(a.x / b.x, a.y / b.y, a.z / b.z, a.w / b.w);
			return false;
		}
	}
}