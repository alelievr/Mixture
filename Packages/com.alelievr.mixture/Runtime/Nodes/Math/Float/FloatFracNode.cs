using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using UnityEngine.Rendering;

namespace Mixture
{
	[Documentation(@"
Extract the fractional part of the input float.
")]

	[System.Serializable, NodeMenuItem("Math/Float Frac")]
	public class FloatFracNode : MixtureNode
	{
		// TODO: multi FloatFrac port
		public override bool hasSettings => false;
		public override bool showDefaultInspector => true;

		[Input("A"), ShowAsDrawer]
		public float a;

		[Output("Out")]
		public float o;

		public override string name => "Frac";
		public override float nodeWidth => MixtureUtils.operatorNodeWidth;

		protected override bool ProcessNode(CommandBuffer cmd)
		{
			o = a % 1.0f;
			return true;
		}
	}
}