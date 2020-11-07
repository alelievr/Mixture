using UnityEngine.Rendering;
using GraphProcessor;

namespace Mixture
{
	[Documentation(@"
Sample a texture. Note that you can use a custom UV texture as well.
")]

	[System.Serializable, NodeMenuItem("Utils/Texture Sample")]
	public class TextureSampleNode : FixedShaderNode
	{
		public override string name => "Texture Sample";

		public override string shaderName => "Hidden/Mixture/TextureSample";

		public override bool displayMaterialInspector => true;

		protected override bool ProcessNode(CommandBuffer cmd)
        {
            if (!base.ProcessNode(cmd))
                return false;

            bool useCustomUV = material.HasTextureBound("_UV", rtSettings.GetTextureDimension(graph));
            material.SetKeywordEnabled("USE_CUSTOM_UV", useCustomUV);
            return true;
        }
	}
}