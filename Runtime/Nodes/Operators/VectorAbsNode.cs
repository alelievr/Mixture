﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Operators/Vector Abs")]
	public class VectorAbsNode : MixtureNode
	{
		// TODO: multi VectorAbs port
		public override bool hasSettings => false;

		[Input("A")]
		public Vector4	a;
		
		[Output("Out")]
		public Vector4	o;

		public override string name => "Abs";
		public override float nodeWidth => MixtureUtils.operatorNodeWidth;

		protected override bool ProcessNode()
		{
			o = new Vector4(Mathf.Abs(a.x), Mathf.Abs(a.y), Mathf.Abs(a.z), Mathf.Abs(a.w));
			return true;
		}
	}
}