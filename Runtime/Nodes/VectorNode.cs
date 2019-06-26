using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Constants/Vector")]
	public class VectorNode : BaseNode
	{
		[Output(name = "Vector")]
		public Vector4 vector = Vector4.one;

		public override string	name => "Vector";

	}
}