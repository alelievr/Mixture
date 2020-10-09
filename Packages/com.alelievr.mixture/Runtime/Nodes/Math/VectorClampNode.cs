using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using UnityEngine.Rendering;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Math/Vector Clamp")]
	public class VectorClampNode : MixtureNode
	{
		// TODO: multi VectorClamp port
		public override bool hasSettings => false;
		public override bool showDefaultInspector => true;

		[Input("A"), ShowAsDrawer]
		public Vector4	a;
		
		[Output("Out"), ShowAsDrawer]
		public Vector4	o;

		public float	min;
		public float	max;

		public override string name => "Clamp";

		protected override bool ProcessNode(CommandBuffer cmd)
		{
			o = new Vector4(Mathf.Clamp(a.x, min, max), Mathf.Clamp(a.y, min, max), Mathf.Clamp(a.z, min, max), Mathf.Clamp(a.w, min, max));
			return true;
		}
	}
}