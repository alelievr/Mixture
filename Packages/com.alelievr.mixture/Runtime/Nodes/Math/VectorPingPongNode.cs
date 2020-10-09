using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using UnityEngine.Rendering;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Math/Vector Ping Pong")]
	public class VectorPingPongNode : MixtureNode
	{
		public override bool hasSettings => false;
		public override bool showDefaultInspector => true;

		[Input("T"), ShowAsDrawer]
		public Vector4	t;
		
		[Output("Out")]
		public Vector4	o;
		
		public Vector4	length = Vector4.one;

		public override string name => "Ping Pong";

		protected override bool ProcessNode(CommandBuffer cmd)
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