using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Utils/Texture")]
	public class TextureNode : BaseNode
	{
		[Output(name = "Texture")]
		public Texture2D texture;

		public Texture2D Texture;

		public override string	name => "Texture2D";

		protected override void Process()
		{
			texture = Texture;
		}
	}
}