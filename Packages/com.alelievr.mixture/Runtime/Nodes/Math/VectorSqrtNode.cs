using UnityEngine;
using GraphProcessor;
using UnityEngine.Rendering;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Operators/Vector Sqrt")]
	public class VectorSqrtNode : MixtureNode, INeedsCPU
	{
		public override bool hasSettings => false;

		[Input("A")]
		public Vector4	a;
		
		[Output("Out")]
		public Vector4	o;

		public override string name => "Sqrt";
		public override float nodeWidth => MixtureUtils.operatorNodeWidth;

		protected override bool ProcessNode(CommandBuffer cmd)
		{
			o = new Vector4(Mathf.Sqrt(a.x), Mathf.Sqrt(a.y), Mathf.Sqrt(a.z), Mathf.Sqrt(a.w));
			return true;
		}
	}
}