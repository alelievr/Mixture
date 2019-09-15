using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Custom/Perlin Noise")]
	public class PerlinNoise : FixedShaderNode
	{
		public override string name => "Perlin Noise";

		public override string shaderName => "Hidden/Mixture/PerlinNoise";

		public override bool displayMaterialInspector => true;

		// Enumerate the list of material properties that you don't want to be turned into a connectable port.
		protected override IEnumerable<string> filteredOutProperties => new string[]{};

		// Override this if you node is not compatible with all dimensions
		// public override List<OutputDimension> supportedDimensions => new List<OutputDimension>() {
		// 	OutputDimension.Texture2D,
		// 	OutputDimension.Texture3D,
		// 	OutputDimension.CubeMap,
		// };

		protected override bool ProcessNode()
		{
			if (!base.ProcessNode())
				return false;

			// Check if the UVs are connected or not:
			var port = inputPorts.Find(p => p.portData.identifier.Contains("_UV_"));
			bool customUVs = port.GetEdges().Count != 0;

			if (customUVs)
				material.EnableKeyword("USE_CUSTOM_UV");
			else
				material.DisableKeyword("USE_CUSTOM_UV");

			return true;
		}
	}
}