using GraphProcessor;
using OpenCvSharp;
using UnityEngine;
using UnityEngine.Rendering;

namespace Mixture.OpenCV
{
    [System.Serializable, NodeMenuItem("OpenCV/Convert OpenCv Mat To Texture")]
    public class ConvertOpenCvMatToTexture : MixtureNode
    {
        [Input] public Mat input;
        [Output] public Texture output;

        public override bool hasPreview => true;
        public override Texture previewTexture => output;

        protected override bool ProcessNode(CommandBuffer cmd)
        {
            if (!base.ProcessNode(cmd) || input == null)
                return false;
            output = input.ConvertToTexture(ColorConversionCodes.BGRA2RGBA, true);
            return true;
        }
    }
}

