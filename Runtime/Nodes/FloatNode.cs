﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Constants/Float")]
	public class FloatNode : MixtureNode
	{
		[Output(name = "Float")]
		public float Float = 1.0f;

		public override bool 	hasSettings => false;
		public override string	name => "Float";
	}
}