using UnityEngine.Rendering;
using GraphProcessor;

namespace Mixture
{
	[Documentation(@"
Sample a texture. Note that you can use a custom UV texture as well.
")]

	[System.Serializable, NodeMenuItem("Textures/Texture Sample")]
	public class TextureSampleNode : FixedShaderNode
	{
		public override string name => "Texture Sample";

		public override string shaderName => "Hidden/Mixture/TextureSample";

		public override bool displayMaterialInspector => true;
	}
}