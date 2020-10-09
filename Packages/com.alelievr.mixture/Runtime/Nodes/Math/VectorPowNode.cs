using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using UnityEngine.Rendering;
using System.Linq;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Math/Vector Pow")]
	public class VectorPowNode : MixtureNode
	{
		public override bool hasSettings => false;
		public override bool showDefaultInspector => true;

		[Input("A"), ShowAsDrawer]
		public Vector4	a;
		
		[Output("Out")]
		public Vector4	o;

		public Vector4	power;

		public override string name => "Pow";
		public override float nodeWidth => MixtureUtils.operatorNodeWidth;

		protected override bool ProcessNode(CommandBuffer cmd)
		{
			o = new Vector4(Mathf.Pow(a.x, power.x), Mathf.Pow(a.y, power.y), Mathf.Pow(a.z, power.z), Mathf.Pow(a.w, power.w));
			return true;
		}
	}
}