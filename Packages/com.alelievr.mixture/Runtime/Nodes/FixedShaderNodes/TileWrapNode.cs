using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using UnityEngine.Rendering;

namespace Mixture
{
    [Documentation(@"
Make the input texture tile by wrapping and blending the borders of the texture.
")]

	[System.Serializable, NodeMenuItem("Matte/Tile & Wrap")]
	public class TileWrapNode : FixedShaderNode
	{
		public override string name => "Tile & Wrap";

		public override string shaderName => "Hidden/Mixture/TileWrap";

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
                        new CustomRenderTextureUpdateZone{
                            needSwap = true,
                            passIndex = 3,
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
                        new CustomRenderTextureUpdateZone{
                            needSwap = true,
                            passIndex = 3,
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

            rtSettings.doubleBuffered = true;

            // Setup the successive passes needed or the blur
            output.SetUpdateZones(updateZones);

            return true;
        }

    }
}