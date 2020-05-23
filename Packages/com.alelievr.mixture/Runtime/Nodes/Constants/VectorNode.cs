using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using UnityEngine.Rendering;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Constants/Vector")]
	public class VectorNode : MixtureNode
	{
		public override bool hasSettings => false;

		[Output(name = "Vector"), SerializeField]
		public Vector4 vector = Vector4.one;

		[Output(name = "X")]
		public float x = 0;
		[Output(name = "Y")]
		public float y = 0;
		[Output(name = "Z")]
		public float z = 0;
		[Output(name = "W")]
		public float w = 0;

		public override float nodeWidth => 150.0f;

		public override string	name => "Vector";

		protected override bool ProcessNode(CommandBuffer cmd)
		{
			x = vector.x;
			y = vector.y;
			z = vector.z;
			w = vector.w;
			return true;
		}
	}
}