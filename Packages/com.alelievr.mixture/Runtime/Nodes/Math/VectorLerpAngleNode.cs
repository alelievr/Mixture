using UnityEngine;
using GraphProcessor;
using UnityEngine.Rendering;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Math/Vector Lerp Angle")]
	public class VectorLerpAngleNode : MixtureNode
	{
		public override bool hasSettings => false;
		public override bool showDefaultInspector => true;

		[Input("A"), ShowAsDrawer]
		public Vector4	a;
		
		[Output("Out")]
		public Vector4	o;

		public Vector4	min = Vector4.zero;
		public Vector4	max = new Vector4(180, 180, 180, 180);

		public override string name => "LerpAngle";
		public override float nodeWidth => MixtureUtils.operatorNodeWidth;

		protected override bool ProcessNode(CommandBuffer cmd)
		{
			o = new Vector4(
				Mathf.LerpAngle(a.x, min.x, max.x),
				Mathf.LerpAngle(a.y, min.y, max.y),
				Mathf.LerpAngle(a.z, min.z, max.z),
				Mathf.LerpAngle(a.w, min.w, max.w)
			);
			return true;
		}
	}
}