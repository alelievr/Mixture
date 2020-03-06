using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Utils/Debug Log")]
	public class DebugLogNode : MixtureNode, INeedsCPU
	{
		[Input("obj")]
		public object	input;

		public override string name => "Debug Log";

		protected override bool ProcessNode()
		{
			Debug.Log(input);
			return true;
		}
	}
}