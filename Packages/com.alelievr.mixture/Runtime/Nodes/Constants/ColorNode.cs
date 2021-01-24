using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
	[Documentation(@"
Color constant value.
")]

	[System.Serializable, NodeMenuItem("Constants/Color")]
	public class ColorNode : MixtureNode
	{
		[Output(name = "Color")]
		new public Color color = Color.white;

		public override bool 	hasSettings => false;
		public override string	name => "Color";
	}
}