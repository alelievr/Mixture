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
        public ConversionFormat external3DFormat = ConversionFormat.RGBA32;
		public override Texture previewTexture => outputTextureSettings.Count > 0 ? (Texture)mainOutput.finalCopyRT : Texture2D.blackTexture;

        public override bool hasSettings => true;

        protected override MixtureRTSettings defaultRTSettings => new MixtureRTSettings
        {
            heightMode = OutputSizeMode.Fixed,
            widthMode = OutputSizeMode.Fixed,
            depthMode = OutputSizeMode.Fixed,
            potSize = (rtSettings.GetTextureDimension(graph) == TextureDimension.Tex3D) ? POTSize._32 : POTSize._1024,
            dimension = OutputDimension.SameAsOutput,
            outputChannels = OutputChannel.SameAsOutput,
            outputPrecision = OutputPrecision.SameAsOutput,
            editFlags = EditFlags.Height | EditFlags.Width| EditFlags.TargetFormat,
            wrapMode = TextureWrapMode.Repeat,
            filterMode = FilterMode.Bilinear,
        };

        protected override void Enable()
        {
            base.Enable();

            // Sanitize the RT Settings for the output node, they must contains only valid information for the output node
            if (rtSettings.dimension == OutputDimension.SameAsOutput)
                rtSettings.dimension = OutputDimension.Texture2D;

            onSettingsChanged += () => { graph.NotifyNodeChanged(this); };
        }

        protected override bool ProcessNode(CommandBuffer cmd)
        {
            if (!base.ProcessNode(cmd))
                return false;

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