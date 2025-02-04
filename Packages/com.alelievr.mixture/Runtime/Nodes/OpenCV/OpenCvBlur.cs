using GraphProcessor;
using OpenCvSharp;
using UnityEngine;
using UnityEngine.Rendering;

namespace Mixture.OpenCV
{
    [System.Serializable, NodeMenuItem("OpenCV/Blur")]
    public class OpenCvBlur : OpenCVNode
    {
        [Input] public Vector2 blurSize = new Vector2(10, 10);
        protected override bool ProcessNode(CommandBuffer cmd)
        {
            if(!base.ProcessNode(cmd) || input == null)
                return false;
            output = input.Clone();
            Cv2.Blur(input, output, new Size(blurSize.x, blurSize.y));
            GetPreview();
            return true;
        }
    }
}