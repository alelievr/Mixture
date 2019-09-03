using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Custom/Blur")]
	public class Blur : FixedShaderNode
	{
		public override string name => "Blur";

		public override string shaderName => "Hidden/Mixture/Blur";

		public override bool displayMaterialInspector => true;

		// Code to generate the gaussian weights:
		// public Blur()
		// {
		// 	int weightsCount = 32;

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