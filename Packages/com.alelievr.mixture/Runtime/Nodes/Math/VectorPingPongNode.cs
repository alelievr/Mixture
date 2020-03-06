using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Operators/Vector Ping Pong")]
	public class VectorPingPongNode : MixtureNode, INeedsCPU
	{
		public override bool hasSettings => false;

		[Input("T")]
		public Vector4	t;
		
		[Output("Out")]
		public Vector4	o;
		
		public Vector4	length = Vector4.one;

		public override string name => "Ping Pong";
		public override bool showDefaultInspector => true;

		protected override bool ProcessNode()
		{
			o = new Vector4(
				Mathf.PingPong(t.x, length.x),
				Mathf.PingPong(t.y, length.y),
				Mathf.PingPong(t.z, length.z),
				Mathf.PingPong(t.w, length.w)
			);
			return true;
		}
	}
}