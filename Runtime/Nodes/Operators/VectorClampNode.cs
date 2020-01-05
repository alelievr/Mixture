﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Operators/Vector Clamp")]
	public class VectorClampNode : MixtureNode, ICPUNode
	{
		// TODO: multi VectorClamp port
		public override bool hasSettings => false;

		[Input("A")]
		public Vector4	a;
		
		[Output("Out")]
		public Vector4	o;

		public float	min;
		public float	max;

		public override string name => "Clamp";
		public override bool showDefaultInspector => true;

		protected override bool ProcessNode()
		{
			o = new Vector4(Mathf.Clamp(a.x, min, max), Mathf.Clamp(a.y, min, max), Mathf.Clamp(a.z, min, max), Mathf.Clamp(a.w, min, max));
			return true;
		}
	}
}