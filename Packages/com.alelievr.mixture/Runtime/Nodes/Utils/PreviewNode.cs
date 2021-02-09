using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using UnityEngine.Rendering;

namespace Mixture
{
	[Documentation(@"
The Preview node allows you to visualize your texture data differently using 3 modes.
- Color, it have a per-channel remap and a gamma option.
- Heightmap, choose between altitude or heat gradients, you also have a height remap option.
- Normal, display your normal in tangent or object space. Additionally, you can also visualize the lighting of your normal map (use the right click to change the position of the light).
")]

	[System.Serializable, NodeMenuItem("Utils/Preview")]
	public class PreviewNode : FixedShaderNode
	{
		// Keep in sync with Mode in PreviewNode.shader
		public enum Mode
		{
			Color,
			Normal,
			Heightmap
		}

		public override string name => "Preview";

		public override string shaderName => "Hidden/Mixture/PreviewNode";

		public override bool displayMaterialInspector => true;

        // Enumerate the list of material properties that you don't want to be turned into a connectable port.
		protected override IEnumerable<string> filteredOutProperties => new string[]{ "_Mode", "_ColorMin", "_ColorMax", "_Tiling", "_Gamma", "_NormalMode", "_LightPosition", "_HeightColorSet", "_HeightChannel", "_HeightMin", "_HeightMax"};

		[CustomPortBehavior(nameof(output))]
		public IEnumerable< PortData > HideOutput(List< SerializableEdge > edges) { yield break; }

		public HistogramData histogramData;
		public Vector2 mousePosition = new Vector2(0.5f, 0.5f);
		public Vector2 lightPosition
		{
			get => material.GetVector("_LightPosition");
			set => material.SetVector("_LightPosition", new Vector4(value.x, value.y, material.GetVector("_LightPosition").z, material.GetVector("_LightPosition").w));
		}
		public float tiling => material.GetFloat("_Tiling");

		public Mode GetMode() => (Mode)material.GetFloat("_Mode");

        protected override void Enable()
        {
            base.Enable();
			HistogramUtility.AllocateHistogramData(256, HistogramMode.Color, out histogramData);
		}

        protected override bool ProcessNode(CommandBuffer cmd)
        {
			material.SetFloat("_SourceMip", previewMip);
            return base.ProcessNode(cmd);
        }

        protected override void Disable()
        {
            base.Disable();
			HistogramUtility.Dispose(histogramData);
		}
	}
}