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
			OutputDimension.Texture3D,
		};

		CustomRenderTexture tmpUVMap;
		Material			tmpUVMaterial;

		~Distance()
		{
			tmpUVMap?.Release();
			CoreUtils.Destroy(tmpUVMaterial);
		}

		protected override bool ProcessNode()
		{
			// Force the double buffering for multi-pass flooding
			rtSettings.doubleBuffered = true;

			if (!base.ProcessNode())
				return false;

			UpdateTempRenderTexture(ref tmpUVMap);

			if (tmpUVMaterial == null)
			{
				tmpUVMaterial = new Material(material);
			}

			tmpUVMap.material = tmpUVMaterial;
			tmpUVMap.shaderPass = 0;
			tmpUVMap.doubleBuffered = false;

			// Setup passes for jump flooding
			int stepCount = Mathf.CeilToInt(Mathf.Log(output.width, 2));
			CustomRenderTextureUpdateZone[] updateZones = new CustomRenderTextureUpdateZone[stepCount + 1];

			for (int i = 0; i < stepCount; i++)
			{
				updateZones[stepCount] = new CustomRenderTextureUpdateZone{
					needSwap = true,
					passIndex = stepCount - i + 1,
					rotation = 0f,
					updateZoneCenter = new Vector3(0.5f, 0.5f, 0.5f),
					updateZoneSize = new Vector3(1f, 1f, 1f),
				};
			};
			// CRT Workaround: we need to add an additional pass because there is a bug in the swap
			// of the double buffered CRTs: the last pudate zone will not be passed to the next CRT in the chain.
			// So we add a dummy pass to force a copy
			updateZones[stepCount] = new CustomRenderTextureUpdateZone{
				needSwap = true,
				passIndex = 1,
				rotation = 0f,
				updateZoneCenter = new Vector3(0.0f, 0.0f, 0.0f),
				updateZoneSize = new Vector3(0f, 0f, 0f),
			};


			tmpUVMap.Update();

			// TODO: handle texture3D
			material.SetTexture("_UVMap", tmpUVMap);

			// Setup the successive passes needed or the blur
			output.SetUpdateZones(updateZones);
			
			return true;
		}
	}
}