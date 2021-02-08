using GraphProcessor;

namespace Mixture
{
	[Documentation(@"
Detect the edges in the input texture, this node uses a Sobel filter to do so.

You can use the mode to either output the edges in black and white or output the edges multiplied by the input color.
")]

	[System.Serializable, NodeMenuItem("Color/Edge Detection")]
	public class EdgeDetect : FixedShaderNode
	{
		public override string name => "Edge Detection";

		public override string shaderName => "Hidden/Mixture/EdgeDetect";

		public override bool displayMaterialInspector => true;
	}
}