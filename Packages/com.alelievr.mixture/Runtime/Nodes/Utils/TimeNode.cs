using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using UnityEngine.Rendering;

namespace Mixture
{
	[Documentation(@"
Output multiple time related values.

This node can be used in a realtime graph for time based effects.
")]

	[System.Serializable, NodeMenuItem("Utils/Time")]
	public class TimeNode : MixtureNode
	{
		[Output(name = "Time")]
		public float				time;

		[Output(name = "Sin Time")]
		public float				sinTime;

		[Output(name = "Cos Time")]
		public float				cosTime;

		[Output(name = "Delta Time")]
		public float				deltaTime;

		[Output(name = "Frame Count")]
		public float				frameCount;

		public override bool 		hasSettings => false;
		public override float		nodeWidth => 120;
		public override string		name => "Time";

		protected override bool ProcessNode(CommandBuffer cmd)
		{
			time = Time.time;
			sinTime = Mathf.Sin(Time.time);
			cosTime = Mathf.Cos(Time.time);
			deltaTime = Time.deltaTime;
			frameCount = Time.frameCount;

			return true;
		}
	}
}