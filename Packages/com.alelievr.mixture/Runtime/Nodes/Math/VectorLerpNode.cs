using UnityEngine;
using GraphProcessor;
using UnityEngine.Rendering;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Operators/Vector Lerp")]
	public class VectorLerpNode : MixtureNode, INeedsCPU
	{
		public override bool hasSettings => false;

		[Input("A")]
		public Vector4	a;
		
		[Output("Out")]
		public Vector4	o;

		public Vector4	min = Vector4.zero;
		public Vector4	max = Vector4.one;

		public override string name => "Lerp";
		public override float nodeWidth => MixtureUtils.operatorNodeWidth;

		protected override bool ProcessNode(CommandBuffer cmd)
		{
			o = new Vector4(
				Mathf.Lerp(min.x, max.x, a.x),
				Mathf.Lerp(min.y, max.y, a.y),
				Mathf.Lerp(min.z, max.z, a.z),
				Mathf.Lerp(min.w, max.w, a.w)
			);
			return true;
		}
	}
}