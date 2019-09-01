using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Operators/Vector Mul")]
	public class VectorMulNode : MixtureNode
	{
		// TODO: Vectormulti VectorMul port

		[Input("A")]
		public Vector4	a;
		
		[Input("B")]
		public Vector4	b;
		
		[Output("Out")]
		public Vector4	o;

		public override string name => "Mul";

		protected override bool ProcessNode()
		{
			o = Vector4.Scale(a, b);
			return false;
		}
	}
}