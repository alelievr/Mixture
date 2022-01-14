using UnityEngine;
using GraphProcessor;
using System;
using System.Linq;
using UnityEngine.Rendering;

namespace Mixture
{
    [Documentation(@"
Export a texture from the graph, the texture can also be exported outside of unity.

Note that for 2D textures, the file is exported either in png or exr depending on the current floating precision.
For 3D and Cube textures, the file is exported as a .asset and can be use in another Unity project.
")]

    [Serializable, NodeMenuItem("External Output")]
    public class ExternalOutputNode : OutputNode
    {
        public enum ExternalOutputDimension
        {
            Texture2D,
            Texture3D,
            Cubemap,
        }
        public enum External2DOutputType
        {
            Color,
            Normal,
            Linear,
            LatLongCubemapColor,
            LatLongCubemapLinear,
        }
        public enum ExternalFileType
        {
            PNG, 
            EXR
        }


        public override string name => "External Output";

        public Texture asset;

        public ExternalOutputDimension externalOutputDimension = ExternalOutputDimension.Texture2D;
        public External2DOutputType external2DOoutputType = External2DOutputType.Color;
        public ExternalFileType externalFileType = ExternalFileType.PNG;
        public ConversionFormat external3DFormat = ConversionFormat.RGBA32;
		public override Texture previewTexture => outputTextureSettings.Count > 0 ? (Texture)mainOutput.finalCopyRT : Texture2D.blackTexture;

        public override bool hasSettings => true;

        public override bool canEditPreviewSRGB => false;

        protected override MixtureSettings defaultSettings
        {
            get
            {
                POTSize size = (settings.GetResolvedTextureDimension(graph) == TextureDimension.Tex3D) ? POTSize._32 : POTSize._1024;
                return new MixtureSettings
                {
                    sizeMode = OutputSizeMode.Absolute,
                    potSize = size,
                    height = (int)size,
                    width = (int)size,
                    depth = (int)size,
                    dimension = OutputDimension.InheritFromParent,
                    outputChannels = OutputChannel.InheritFromParent,
                    outputPrecision = OutputPrecision.InheritFromParent,
                    editFlags = EditFlags.Height | EditFlags.Width| EditFlags.TargetFormat,
                    wrapMode = OutputWrapMode.Repeat,
                    filterMode = OutputFilterMode.Bilinear,
                };
            }
        }

        protected override void Enable()
        {
            base.Enable();

            onSettingsChanged += () => { graph.NotifyNodeChanged(this); };
        }

        protected override bool ProcessNode(CommandBuffer cmd)
        {
            if (!base.ProcessNode(cmd))
                return false;

            uniqueMessages.Clear();

            if(graph.type != MixtureGraphType.Realtime)
            {
                if(settings.GetResolvedTextureDimension(graph) != TextureDimension.Cube)
                {
                    outputTextureSettings.First().sRGB = false;
                    return base.ProcessNode(cmd);
                }
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