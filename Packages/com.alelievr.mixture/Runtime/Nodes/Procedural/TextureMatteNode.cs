using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
	[Documentation(@"
Sample a Texture with a scale and a bias on the UVs.
This node can be useful to check if a texture is tiling by putting the scale to 2.
")]

	[System.Serializable, NodeMenuItem("Matte/Texture Matte")]
	public class TextureMatteNode : FixedShaderNode
	{
		public override string name => "Texture Matte";

		public override string shaderName => "Hidden/Mixture/TextureMatte";

		public override bool displayMaterialInspector => true;
	}
}