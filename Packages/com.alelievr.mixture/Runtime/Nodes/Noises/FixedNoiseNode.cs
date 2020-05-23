using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using UnityEngine.Rendering;

namespace Mixture
{
	[System.Serializable]
	public abstract class FixedNoiseNode : FixedShaderNode
	{
		public override bool displayMaterialInspector => true;

		public override PreviewChannels defaultPreviewChannels => PreviewChannels.RGB; // Hide alpha channel for noise preview

		// Enumerate the list of material properties that you don't want to be turned into a connectable port.
		protected override IEnumerable<string> filteredOutProperties => new string[]{ "_Seed", "_OutputRange", "_TilingMode", "_CellSize", "_Octaves"};

		protected override bool ProcessNode(CommandBuffer cmd)
		{
			if (!base.ProcessNode(cmd))
				return false;

			// Check if the UVs are connected or not:
			var port = inputPorts.Find(p => p.portData.identifier.Contains("_UV_"));
			if (port == null)
				return false;

			bool customUVs = port.GetEdges().Count != 0;

			if (customUVs)
				material.EnableKeyword("USE_CUSTOM_UV");
			else
				material.DisableKeyword("USE_CUSTOM_UV");

			if (material.IsKeywordEnabled("_TILINGMODE_TILED"))
			{
				material.SetFloat("_Lacunarity", Mathf.Round(material.GetFloat("_Lacunarity")));
				material.SetFloat("_Frequency", Mathf.Round(material.GetFloat("_Frequency")));
			}

			return true;
		}
	}
}