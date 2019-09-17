using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
	[System.Serializable]
	public abstract class FixedNoiseNode : FixedShaderNode
	{
		public override bool displayMaterialInspector => true;

		// Enumerate the list of material properties that you don't want to be turned into a connectable port.
		protected override IEnumerable<string> filteredOutProperties => new string[]{ "_Seed", "_OutputRange" };

		protected override bool ProcessNode()
		{
			if (!base.ProcessNode())
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

			return true;
		}
	}
}