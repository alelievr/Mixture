﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
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
		public override float		nodeWidth => 70;
		public override string		name => "Time";

		protected override bool ProcessNode()
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