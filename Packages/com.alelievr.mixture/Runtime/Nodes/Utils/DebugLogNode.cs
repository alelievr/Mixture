using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using UnityEngine.Rendering;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Utils/Debug Log")]
	public class DebugLogNode : MixtureNode
	{
		[Input("obj")]
		public object	input;

		public override string name => "Debug Log";

		protected override bool ProcessNode(CommandBuffer cmd)
		{
			Debug.Log(input);
			return true;
		}
	}
}