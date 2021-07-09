using System.Collections;

using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using UnityEngine.Rendering;

namespace Mixture
{
	[Documentation(@"
Gaussian blur filter in two passes. You might see some artifacts with large blur values because there is a fixed amount of samples (64) in the shader.
")]

	[System.Serializable, NodeMenuItem("Operators/Blur")]
	public class Blur : FixedShaderNode
	{
		public override string name => "Blur";

		public override string shaderName => "Hidden/Mixture/Blur";

		public override bool displayMaterialInspector => true;

		protected override bool ProcessNode(CommandBuffer cmd)
		{
			if (!base.ProcessNode(cmd))
				return false;

			CustomRenderTextureUpdateZone[] updateZones;
		
			// Setup the custom render texture multi-pass for the blur:
			switch (output.dimension)
			{
				default:
				case TextureDimension.Tex2D:
				case TextureDimension.Cube:
					updateZones = new CustomRenderTextureUpdateZone[] {
						new CustomRenderTextureUpdateZone{
							needSwap = false,
							passIndex = 0,
							rotation = 0f,
                    		updateZoneCenter = new Vector3(0.5f, 0.5f, 0.5f),
                    		updateZoneSize = new Vector3(1f, 1f, 1f),
						},
						new CustomRenderTextureUpdateZone{
							needSwap = true,
							passIndex = 1,
							rotation = 0f,
                    		updateZoneCenter = new Vector3(0.5f, 0.5f, 0.5f),
                    		updateZoneSize = new Vector3(1f, 1f, 1f),
						},
						// CRT Workaround: we need to add an additional pass because there is a bug in the swap
						// of the double buffered CRTs: the last pudate zone will not be passed to the next CRT in the chain.
						// So we add a dummy pass to force a copy
						new CustomRenderTextureUpdateZone{
							needSwap = true,
							passIndex = 1,
							rotation = 0f,
                    		updateZoneCenter = new Vector3(0.0f, 0.0f, 0.0f),
                    		updateZoneSize = new Vector3(0f, 0f, 0f),
						},
					};
					break;
				case TextureDimension.Tex3D:
					updateZones = new CustomRenderTextureUpdateZone[] {
						new CustomRenderTextureUpdateZone{
							needSwap = false,
							passIndex = 0,
							rotation = 0f,
                    		updateZoneCenter = new Vector3(0.5f, 0.5f, 0.5f),
                    		updateZoneSize = new Vector3(1f, 1f, 1f),
						},
						new CustomRenderTextureUpdateZone{
							needSwap = true,
							passIndex = 1,
							rotation = 0f,
                    		updateZoneCenter = new Vector3(0.5f, 0.5f, 0.5f),
                    		updateZoneSize = new Vector3(1f, 1f, 1f),
						},
						new CustomRenderTextureUpdateZone{
							needSwap = true,
							passIndex = 2,
							rotation = 0f,
                    		updateZoneCenter = new Vector3(0.5f, 0.5f, 0.5f),
                    		updateZoneSize = new Vector3(1f, 1f, 1f),
						},
						// CRT Workaround: we need to add an additional pass because there is a bug in the swap
						// of the double buffered CRTs: the last pudate zone will not be passed to the next CRT in the chain.
						// So we add a dummy pass to force a copy
						new CustomRenderTextureUpdateZone{
							needSwap = true,
							passIndex = 1,
							rotation = 0f,
                    		updateZoneCenter = new Vector3(0.0f, 0.0f, 0.0f),
                    		updateZoneSize = new Vector3(0f, 0f, 0f),
						},
					};
					break;
			}

			settings.doubleBuffered = true;

			output.EnsureDoubleBufferConsistency();
			var rt = output.GetDoubleBufferRenderTexture();
			var t = material.GetTextureWithDimension("_Source", settings.GetResolvedTextureDimension(graph));
			if (rt != null && t != null)
			{
				rt.filterMode = t.filterMode;
				rt.wrapMode = t.wrapMode;
			}

			// Setup the successive passes needed or the blur
			output.SetUpdateZones(updateZones);

			return true;
		}

		// Code to generate the gaussian weights:
		// public Blur()
		// {
		// 	int weightsCount = 64;

		// 	string weightsArray = $"static float gaussianWeights[{weightsCount}] = {{";
		// 	float p = 0;
		// 	float wTotal = 0;
		// 	float integrationBound = 3;
		// 	for (int i = 0; i < weightsCount; i++)
		// 	{
		// 		float w = (Gaussian(p) / (float)weightsCount) * integrationBound;
		// 		p += 1.0f / (float)weightsCount * integrationBound;
		// 		weightsArray += w.ToString() + ",\n";
		// 		wTotal += w;
		// 	}
		// 	weightsArray += "};";

		// 	Debug.Log(weightsArray);

		// 	// Gaussian weights:
		// 	float Gaussian(float x, float sigma = 1)
		// 	{
		// 		float a = 1.0f / Mathf.Sqrt(2 * Mathf.PI * sigma * sigma);
		// 		float b = Mathf.Exp(-(x * x) / (2 * sigma * sigma));
		// 		return a * b;
		// 	}
		// }
	}
}