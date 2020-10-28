using UnityEngine;
using GraphProcessor;
using UnityEngine.Rendering;

namespace Mixture
{
		[Documentation(@"
Perform a lerp between min and max vectors.
")]

	[System.Serializable, NodeMenuItem("Math/Vector Lerp")]
	public class VectorLerpNode : MixtureNode
	{
		public override bool hasSettings => false;
		public override bool showDefaultInspector => true;

		[Input("A"), ShowAsDrawer]
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