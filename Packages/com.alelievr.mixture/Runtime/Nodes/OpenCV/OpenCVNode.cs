using GraphProcessor;
using OpenCvSharp;
using UnityEngine;

namespace Mixture.OpenCV
{
    [System.Serializable]
    public class OpenCVNode : MixtureNode
    {
        [Input] public Mat input;
        [Output]public Mat output;
        protected Texture2D preview;
        
        public override Texture previewTexture => preview;
        
        public override bool hasPreview => true;

        /// <summary>
        /// Compute the preview texture by converting the output Mat to a Texture2D
        /// Only call in the Editor
        /// Make it virtual so the conversion process could be overriden
        /// Should it be move to a separate Editor class?
        /// </summary>
        protected virtual void GetPreview()
        {
            #if UNITY_EDITOR
            preview = output?.ConvertToTexture(ColorConversionCodes.BGRA2RGBA, true);
            #endif
        }
    }
}