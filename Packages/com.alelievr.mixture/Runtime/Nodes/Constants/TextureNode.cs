using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Constants/Texture")]
	public class TextureNode : MixtureNode
	{
		[Output(name = "Texture"), SerializeField]
		public Texture2D texture;

		public override bool 	hasSettings => false;
		public override string	name => "Texture2D";
        public override Texture previewTexture => texture;
		public override bool	showDefaultInspector => true;
    }
}