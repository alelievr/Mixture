using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Custom/Suibgraph")]
	public class Suibgraph : MixtureNode
	{
		[Input]
		public Texture2D input;
		[Output]
		public Texture2D output;

		public override string name => "Suibgraph";
	}
}