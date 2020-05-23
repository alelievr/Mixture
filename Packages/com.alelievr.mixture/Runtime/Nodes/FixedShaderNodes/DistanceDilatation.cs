using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
	// [System.Serializable, NodeMenuItem("Custom/DistanceDilatation")]
	public class DistanceDilatation : FixedShaderNode
	{
		public override string name => "DistanceDilatation";

		public override string shaderName => "Hidden/Mixture/DistanceDilatation";

		public override bool displayMaterialInspector => true;
		public override bool showDefaultInspector => true;

		public int passCount = 10;

		// Enumerate the list of material properties that you don't want to be turned into a connectable port.
		protected override IEnumerable<string> filteredOutProperties => new string[]{};
		
		public override List<OutputDimension> supportedDimensions => new List<OutputDimension>() {
			OutputDimension.Texture2D,
			OutputDimension.Texture3D,
		};

		// RenderTexture dilatationBuffer;
		List<CustomRenderTextureUpdateZone> updateZones = new List<CustomRenderTextureUpdateZone>();
		
		protected override void Enable()
		{
			base.Enable();
		}

		protected override void Disable()
		{
			base.Disable();
		}

		protected override bool ProcessNode(CommandBuffer cmd)
		{
			if (!base.ProcessNode(cmd))
				return false;
			
			// TODO: force the CRT format to GraphicsFormat.R16G16B16_SFloat, we need this precision for the dilatation buffer

			// The upper limit for render target size on this node is 32K
			// Format used in this render target: 1 bit for dilatation marker, the others bits for the 3d origin position of the dilatation| 
			// UpdateRenderTextureSize(ref dilatationBuffer, GraphicsFormat.R16G16B16_SFloat);
			
			// switch (rtSettings.GetTextureDimension(graph))
			// {
			// 	case TextureDimension.Tex2D:
			// 		material.SetTexture("_DilatationBuffer_2D", dilatationBuffer);
			// 		break;
			// 	case TextureDimension.Tex3D:
			// 		material.SetTexture("_DilatationBuffer_3D", dilatationBuffer);
			// 		break;
			// 	default:
			// 		material.SetTexture("_DilatationBuffer_Cube", dilatationBuffer);
			// 		break;
			// }

			updateZones.Clear();

			// First we clear all the buffers:
			updateZones.Add(new CustomRenderTextureUpdateZone
            {
                needSwap = false,
                passIndex = 0,
                rotation = 0f,
                updateZoneCenter = new Vector3(0.5f, 0.5f, 0.5f),
                updateZoneSize = new Vector3(1f, 1f, 1f),
            });

			// Do the actual dilatation passes:
			for (int i = 0; i < passCount; i++)
			{
				// Dilatation X:
                updateZones.Add(new CustomRenderTextureUpdateZone
                {
                    needSwap = true,
                    passIndex = 1,
                    rotation = 0f,
                    updateZoneCenter = new Vector3(0.5f, 0.5f, 0.5f),
                    updateZoneSize = new Vector3(1f, 1f, 1f),
                });
				
				// Dilatation Y:
                updateZones.Add(new CustomRenderTextureUpdateZone
                {
                    needSwap = true,
                    passIndex = 2,
                    rotation = 0f,
                    updateZoneCenter = new Vector3(0.5f, 0.5f, 0.5f),
                    updateZoneSize = new Vector3(1f, 1f, 1f),
                });

				// TODO: dilatation Z if we're processing a 3d texture
			}

			// CRT Workaround: we need to add an additional pass because there is a bug in the swap
			// of the double buffered CRTs: the last pudate zone will not be passed to the next CRT in the chain.
			// So we add a dummy pass to force a copy
            updateZones.Add(new CustomRenderTextureUpdateZone
            {
                needSwap = true,
                passIndex = 1,
                rotation = 0f,
                updateZoneCenter = new Vector3(0.0f, 0.0f, 0.0f),
                updateZoneSize = new Vector3(0f, 0f, 0f),
            });

			output.SetUpdateZones(updateZones.ToArray());
			output.doubleBuffered = true;

			return true;
		}
	}
}