using GraphProcessor;

namespace Mixture
{
	[Documentation(@"
Sharpen the input image using a very simple 3x3 sharpening kernel.
")]

	[System.Serializable, NodeMenuItem("Color/Sharpen")]
	public class Sharpen : FixedShaderNode
	{
		public override string name => "Sharpen";

		public override string shaderName => "Hidden/Mixture/Sharpen";

		public override bool displayMaterialInspector => true;
	}
}