using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEngine;
using GraphProcessor;

namespace Mixture
{
	[Documentation(@"
Convert RGB image to White and Black. With the Mode property you can change how the black and white color is computed:

Name | Description
--- | ---
Perceptual Luminance | Compute the luminance with this RGB factor: 0.299, 0.587, 0.114
D65 Luminance | Compute the luminance with D65 standard RGB factor: 0.2126729, 0.7151522, 0.0721750
Custom Luminance | Compute the luminance the custom value ""Lum Factors"" in the inspector.
Lightness | Compute the lightness with `( max(R, G, B) + min(R, G, B) ) / 2`
Average | Compute the average with `( R + G + B ) / 3`
")]

	[System.Serializable, NodeMenuItem("Color/Black And White")]
	public class BlackAndWhiteNode : FixedShaderNode
	{
		public override string name => "Black And White";

		public override string shaderName => "Hidden/Mixture/BlackAndWhite";

		public override bool displayMaterialInspector => true;

		protected override IEnumerable<string> filteredOutProperties => new string[]{"_ColorNorm", "_LuminanceMode"};

		protected override bool ProcessNode(CommandBuffer cmd)
		{
			if (!base.ProcessNode(cmd))
				return false;

			switch (material.GetFloat("_LuminanceMode"))
			{
				case 0: // Perceptual
					material.SetVector("_ColorNorm", new Vector4(0.299f, 0.587f, 0.114f, 1));
					break;
				case 1: // D65
					material.SetVector("_ColorNorm", new Vector4(0.2126729f, 0.7151522f, 0.0721750f, 1));
					break;
			}

			return true;
		}
    }
}