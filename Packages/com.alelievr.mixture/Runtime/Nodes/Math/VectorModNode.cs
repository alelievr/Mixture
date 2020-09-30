using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using UnityEngine.Rendering;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Operators/Vector Mod")]
	public class VectorModNode : MixtureNode
	{
		// TODO: multi VectorMod port
		public override bool hasSettings => false;

		[Input("A")]
		public Vector4	a;
		
		[Input("B")]
		public Vector4	b;
		
		[Output("Out")]
		public Vector4	o;

		public override string name => "Mod";
		public override float nodeWidth => MixtureUtils.operatorNodeWidth;

		protected override bool ProcessNode(CommandBuffer cmd)
		{
			o = new Vector4(a.x % b.x, a.y % b.y, a.z % b.z, a.w % b.w);
			return true;
		}
	}
}