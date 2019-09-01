using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using System;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Operators/VectorTruncate")]
	public class VectorTruncateNode : MixtureNode
	{
		// TODO: multi VectorTruncate port

		[Input("A")]
		public Vector4	a;
		
		[Output("Out")]
		public Vector4	o;

		public override string name => "Truncate";

		protected override bool ProcessNode()
		{
			o = new Vector4((float)Math.Truncate((double)a.x), (float)Math.Truncate((double)a.y), (float)Math.Truncate((double)a.z), (float)Math.Truncate((double)a.w));
			return false;
		}
	}
}