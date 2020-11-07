using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using UnityEngine.UIElements;

namespace Mixture
{
	[System.Serializable, NodeCustomEditor(typeof(SplatterNode))]
	public class SplatterNodeView : ComputeShaderNodeView
	{
		SplatterNode node;

		// public override void Enable()
		// {
		// 	base.Enable(fromInspector);

		// 	node = nodeTarget as SplatterNode;
		// }
	}
}