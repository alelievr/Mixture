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
		protected override IEnumerable<string> filteredOutProperties => new string[]{ "_OutputRange", "_TilingMode", "_CellSize", "_Octaves", "_Channels"};

		protected override bool ProcessNode(CommandBuffer cmd)
		{
			if (!base.ProcessNode(cmd))
				return false;

			// Check if we need to use custom UVs or not 
            bool useCustomUV = material.HasTextureBound("_UV", rtSettings.GetTextureDimension(graph));
			material.SetKeywordEnabled("USE_CUSTOM_UV", useCustomUV);

			if (material.IsKeywordEnabled("_TILINGMODE_TILED"))
			{
				material.SetFloat("_Lacunarity", Mathf.Round(material.GetFloat("_Lacunarity")));
				material.SetFloat("_Frequency", Mathf.Round(material.GetFloat("_Frequency")));
			}

			return true;
		}
	}
}