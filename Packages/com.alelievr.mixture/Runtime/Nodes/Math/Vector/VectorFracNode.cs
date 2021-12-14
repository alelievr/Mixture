using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using UnityEngine.Rendering;

namespace Mixture
{
	[Documentation(@"
Extract the fractional part of the input vector.
")]

	[System.Serializable, NodeMenuItem("Math/Vector Frac")]
	public class VectorFracNode : MixtureNode
	{
		// TODO: multi VectorFrac port
		public override bool hasSettings => false;
		public override bool showDefaultInspector => true;

		[Input("A"), ShowAsDrawer]
		public Vector4	a;
		
		[Output("Out"), ShowAsDrawer]
		public Vector4	o;

		public override string name => "Frac";
		public override float nodeWidth => MixtureUtils.operatorNodeWidth;

		protected override bool ProcessNode(CommandBuffer cmd)
		{
			o = new Vector4(a.x % 1.0f, a.y % 1.0f, a.z % 1.0f, a.w % 1.0f);
			return true;
		}
	}
}