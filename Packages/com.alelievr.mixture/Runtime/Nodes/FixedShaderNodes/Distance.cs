using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using UnityEngine.Rendering;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Custom/Distance")]
	public class Distance : FixedShaderNode
	{
		public override string name => "Distance";

		public override string shaderName => "Hidden/Mixture/Distance";

		public override bool displayMaterialInspector => true;

		// Enumerate the list of material properties that you don't want to be turned into a connectable port.
		protected override IEnumerable<string> filteredOutProperties => new string[]{};

		public override List<OutputDimension> supportedDimensions => new List<OutputDimension>() {
			OutputDimension.Texture2D,
		};

		protected override bool ProcessNode(CommandBuffer cmd)
		{
			// Force the double buffering for multi-pass flooding
			rtSettings.doubleBuffered = true;

			if (!base.ProcessNode(cmd))
				return false;

			// Setup passes for jump flooding
			int stepCount = Mathf.CeilToInt(Mathf.Log(output.width, 2));
			CustomRenderTextureUpdateZone[] updateZones = new CustomRenderTextureUpdateZone[stepCount + 3];

			updateZones[0] = new CustomRenderTextureUpdateZone{
				needSwap = false,
				passIndex = 0,
				rotation = 0f,
				updateZoneCenter = new Vector3(0.5f, 0.5f, 0.5f),
				updateZoneSize = new Vector3(1f, 1f, 1f),
			};

			for (int i = 0; i < stepCount; i++)
			{
				updateZones[i + 1] = new CustomRenderTextureUpdateZone{
					needSwap = true,
					passIndex = stepCount - i + 1,
					rotation = 0f,
					updateZoneCenter = new Vector3(0.5f, 0.5f, 0.5f),
					updateZoneSize = new Vector3(1f, 1f, 1f),
				};
			};

			updateZones[stepCount + 1] = new CustomRenderTextureUpdateZone{
				needSwap = true,
				passIndex = 1,
				rotation = 0f,
				updateZoneCenter = new Vector3(0.5f, 0.5f, 0.5f),
				updateZoneSize = new Vector3(1f, 1f, 1f),
			};

			// CRT Workaround: we need to add an additional pass because there is a bug in the swap
			// of the double buffered CRTs: the last pudate zone will not be passed to the next CRT in the chain.
			// So we add a dummy pass to force a copy
			updateZones[stepCount + 2] = new CustomRenderTextureUpdateZone{
				needSwap = true,
				passIndex = 1,
				rotation = 0f,
				updateZoneCenter = new Vector3(0.0f, 0.0f, 0.0f),
				updateZoneSize = new Vector3(0f, 0f, 0f),
			};

			material.SetFloat("_SourceMipCount", Mathf.Log(output.width, 2));
			output.SetUpdateZones(updateZones);
			
			return true;
		}
	}
}