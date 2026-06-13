using System.Runtime.InteropServices;
using GraphProcessor;
using OpenCvSharp;
using UnityEngine;
using UnityEngine.Rendering;

namespace Mixture.OpenCV
{
    [System.Serializable, NodeMenuItem("OpenCV/Convert Texture to OpenCV")]
    public class ConvertTextureToOpenCV : MixtureNode
    {
        [Input] public Texture input;

        [Output] public Mat output;
        private Texture preview;

        public override Texture previewTexture => preview;
        public override bool hasPreview => true;

        protected override bool ProcessNode(CommandBuffer cmd)
        {
            if(!base.ProcessNode(cmd) || input == null)
                return false;
            MixtureGraphProcessor.AddGPUAndCPUBarrier(cmd);
            Debug.Log(input.isDataSRGB);
            output = input.ConvertToMat(ColorConversionCodes.RGBA2BGRA, flip:true);
            #if UNITY_EDITOR
            preview = output?.ConvertToTexture(ColorConversionCodes.BGRA2RGBA, true);
            #endif
            return true;
        }
    }
}