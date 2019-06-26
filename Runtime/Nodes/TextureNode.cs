using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Constants/Texture")]
	public class TextureNode : BaseNode
	{
		[Output(name = "Texture")]
		public Texture2D texture;

		public override string	name => "Texture2D";

	}
}