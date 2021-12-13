using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using UnityEngine.Rendering;

namespace Mixture
{
	[Documentation(@"
Computes the ceilng of the input float.
")]

	[System.Serializable, NodeMenuItem("Math/Float Ceil")]
	public class FloatCeilNode : MixtureNode
	{
		// TODO: multi FloatFrac port
		public override bool hasSettings => false;
		public override bool showDefaultInspector => true;

		[Input("A"), ShowAsDrawer]
		public float a;

		[Output("Out")]
		public float o;

		public override string name => "Ceil";
		public override float nodeWidth => MixtureUtils.operatorNodeWidth;

		protected override bool ProcessNode(CommandBuffer cmd)
		{
			o = Mathf.Ceil(a);
			return true;
		}
	}
}