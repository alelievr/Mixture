using UnityEngine;
using GraphProcessor;
using System;
using System.Linq;
using UnityEngine.Rendering;

namespace Mixture
{
    [Serializable, NodeMenuItem("External Output")]
    public class ExternalOutputNode : OutputNode
    {
        public enum ExternalOutputDimension
        {
            Texture2D,
            Texture3D
        }
        public enum External2DOutputType
        {
            Color,
            Normal,
            Linear,
            LatLonCubemap
        }

        public override string name => "External Output";

        public Texture asset;

        public ExternalOutputDimension externalOutputDimension = ExternalOutputDimension.Texture2D;
        public External2DOutputType external2DOoutputType = External2DOutputType.Color;

        public override bool hasSettings => true;

        protected override MixtureRTSettings defaultRTSettings => new MixtureRTSettings
        {
            heightMode = OutputSizeMode.Fixed,
            widthMode = OutputSizeMode.Fixed,
            depthMode = OutputSizeMode.Fixed,
            height = 512,
            width = 512,
            sliceCount = 1,
            dimension = OutputDimension.SameAsOutput,
            outputChannels = OutputChannel.SameAsOutput,
            outputPrecision = OutputPrecision.SameAsOutput,
            editFlags = EditFlags.Height | EditFlags.Width| EditFlags.TargetFormat,
            wrapMode = TextureWrapMode.Repeat,
            filterMode = FilterMode.Bilinear,
        };

        protected override void Enable()
        {
            // Do NOT Call base.Enable() as it references the node as the main output of the graph.
            //base.Enable(fromInspector);

            // Sanitize the RT Settings for the output node, they must contains only valid information for the output node
            if (rtSettings.dimension == OutputDimension.SameAsOutput)
                rtSettings.dimension = OutputDimension.Texture2D;

            Debug.Log("TODO!");

            // if (graph.isRealtime)
            // {
            //     tempRenderTexture = graph.outputTexture as CustomRenderTexture;
            // }
            // else
            // {
            //     UpdateTempRenderTexture(ref tempRenderTexture);
            // }

			// if (finalCopyMaterial == null)
			// {
			// 	var shader = Shader.Find("Hidden/Mixture/FinalCopy");

			// 	// shader can be null if this function is called during the import of the package
			// 	if (shader != null)
			// 	{
			// 		finalCopyMaterial = new Material(shader);
			// 		finalCopyMaterial.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
			// 	}
			// }

            // tempRenderTexture.material = finalCopyMaterial;

            // TODO: add this for every mixture node
            onSettingsChanged += () => { graph.NotifyNodeChanged(this); };
        }

        protected override void Disable() => CoreUtils.Destroy(outputTextureSettings.First().finalCopyRT);

        protected override bool ProcessNode(CommandBuffer cmd)
        {
            uniqueMessages.Clear();

            if(!graph.isRealtime)
            {
                if(rtSettings.dimension != OutputDimension.CubeMap)
                    return base.ProcessNode(cmd);
                else
                {
                    if (uniqueMessages.Add("CubemapNotSupported"))
                        AddMessage("Using texture cubes with this node is not supported.", NodeMessageType.Warning);
                    return false;
                }

            }
            else
            {
                if (uniqueMessages.Add("RealtimeNotSupported"))
                    AddMessage("Using this node in a real-time mixture graph is not supported.", NodeMessageType.Warning);
                return false;
            }
        }

    }
}