using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using UnityEngine.Rendering;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Utils/Vector Swizzle")]
	public class VectorSwizzle : MixtureNode
	{
		[Input("Input")]
		public Vector4	input;

		[Output]
		public Vector4	output;
		
		public override bool showDefaultInspector => true;

		public Component	compX = Component.X;
		public Component	compY = Component.Y;
		public Component	compZ = Component.Z;
		public Component	compW = Component.W;
		public float		custom;

		public enum Component
		{
			X,
			Y,
			Z,
			W,
			Custom,
		}

		IEnumerable< PortData > CustomOutputPort(List< SerializableEdge > edges)
		{
			yield return new PortData {
				identifier = "Output",
				displayName = $"{compX.ToString()[0]}{compY.ToString()[0]}{compZ.ToString()[0]}{compW.ToString()[0]}",
				displayType = typeof(Vector4),
			};
		}

		public override string name => "Vector Swizzle";

		float Swizzle(Component c, Vector4 v)
		{
			switch (c)
			{
				case Component.X: return v.x;
				case Component.Y: return v.y;
				case Component.Z: return v.z;
				case Component.W: return v.w;
				default: case Component.Custom: return custom;
			}
		}

		protected override bool ProcessNode(CommandBuffer cmd)
		{
			output.x = Swizzle(compX, input);
			output.y = Swizzle(compY, input);
			output.z = Swizzle(compZ, input);
			output.w = Swizzle(compW, input);
			return true;
		}
	}
}