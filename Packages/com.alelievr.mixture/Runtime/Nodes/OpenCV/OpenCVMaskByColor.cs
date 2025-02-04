using GraphProcessor;
using OpenCvSharp;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Mixture.OpenCV
{
    [System.Serializable, NodeMenuItem("OpenCV/Mask Generator")]
    public class OpenCVMaskByColor : OpenCVNode
    {
        [Tooltip("[0-255] Range")] public Vector4 fromColor = new Vector4(0, 0, 0, 0);
        [Tooltip("[0-255] Range")] public Vector4 toColor = new Vector4(255, 255, 255, 255);
        
        public override bool showDefaultInspector => true;
        public override bool needsInspector => true;
        
        protected override bool ProcessNode(CommandBuffer cmd)
        {
            if(!base.ProcessNode(cmd) || input == null)
                return false;
            
            output = new Mat(input.Size(), MatType.CV_8UC1);
            var from = new Scalar(fromColor.x, fromColor.y, fromColor.z, fromColor.w);
            var to = new Scalar(toColor.x, toColor.y, toColor.z, toColor.w);
            Debug.Log(from.Val0);
            Debug.Log(to.Val0);
            Cv2.InRange(input, from, to, output);
            GetPreview();
            return true;
        }
    }
    
    
}