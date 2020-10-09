using UnityEngine;
using GraphProcessor;
using UnityEngine.Rendering;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Math/Vector InverseLerp")]
	public class VectorInverseLerpNode : MixtureNode
	{
		public override bool hasSettings => false;
		public override bool showDefaultInspector => true;

		[Input("A"), ShowAsDrawer]
		public Vector4	a;
		
		[Output("Out")]
		public Vector4	o;

		public Vector4	min = Vector4.zero;
		public Vector4	max = Vector4.one;

		public override string name => "InverseLerp";
		public override float nodeWidth => MixtureUtils.operatorNodeWidth;

		protected override bool ProcessNode(CommandBuffer cmd)
		{
			o = new Vector4(
				Mathf.InverseLerp(min.x, max.x, a.x),
				Mathf.InverseLerp(min.y, max.y, a.y),
				Mathf.InverseLerp(min.z, max.z, a.z),
				Mathf.InverseLerp(min.w, max.w, a.w)
			);
			return true;
		}
	}
}