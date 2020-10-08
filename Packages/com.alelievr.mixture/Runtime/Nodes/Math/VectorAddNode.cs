using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using UnityEngine.Rendering;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Math/Vector Add")]
	public class VectorAddNode : MixtureNode
	{
		// TODO: multi Vectoradd port
		public override bool hasSettings => false;

		[Input("A")]
		public Vector4	a;
		
		[Input("B")]
		public Vector4	b;
		
		[Output("Out")]
		public Vector4	o;

		public override string name => "Add";
		public override float nodeWidth => MixtureUtils.operatorNodeWidth;

		protected override bool ProcessNode(CommandBuffer cmd)
		{
			o = a + b;
			return true;
		}
	}
}