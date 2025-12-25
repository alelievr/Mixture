using UnityEngine;
using GraphProcessor;
using UnityEngine.Rendering;

namespace Mixture
{
	[Documentation(@"
When processed, this node will do a Debug.Log() of it's connected input, this can be useful to debug a graph.
")]

	[System.Serializable, NodeMenuItem("Utils/Debug Log")]
	public class DebugLogNode : MixtureNode
	{
		[Input("obj")]
		public object	input;

		public override string name => "Debug Log";

		protected override bool ProcessNode(CommandBuffer cmd)
		{
      if (input is Object o) {
        Debug.Log(input, o);
      } else {
        Debug.Log(input);
      }

			return true;
		}
	}
}
