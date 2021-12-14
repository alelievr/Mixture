using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using UnityEngine.Rendering;

namespace Mixture
{
	[Documentation(@"
Multiplies two input floats.
")]

	[System.Serializable, NodeMenuItem("Math/Float Divide")]
	public class FloatDivNode : MixtureNode
	{
		// TODO: multi FloatFrac port
		public override bool hasSettings => false;
		public override bool showDefaultInspector => true;

		[Input("A"), ShowAsDrawer]
		public float a = 1;

		[Input("B"), ShowAsDrawer]
		public float b = 2;

		[Output("Out")]
		public float o;

		public override string name => "Divide";
		public override float nodeWidth => MixtureUtils.operatorNodeWidth;

		protected override bool ProcessNode(CommandBuffer cmd)
		{
			o = a / b;
			return true;
		}
	}
}