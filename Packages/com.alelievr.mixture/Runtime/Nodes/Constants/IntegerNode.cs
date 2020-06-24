using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Constants/Integer")]
	public class IntegerNode : MixtureNode
	{
		[Output(name = "Integer")]
		public int Int = 1;

		public override bool 	hasSettings => false;
		public override string	name => "Integer";
		public override float	nodeWidth => 200;
	}
}