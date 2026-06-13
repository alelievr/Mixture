using GraphProcessor;
using OpenCvSharp;
using UnityEngine.Rendering;

namespace Mixture.OpenCV
{
    [System.Serializable, NodeMenuItem("OpenCV/Webcam Capture")]
    public class OpenCvWebcamCapture : OpenCVNode
    {
        private VideoCapture capture;
        
        protected override void Enable()
        {
            capture = new VideoCapture(0, VideoCaptureAPIs.DSHOW);
            capture.Set(VideoCaptureProperties.FrameWidth, 1920);
            capture.Set(VideoCaptureProperties.FrameHeight, 1080);
        }

        protected override bool ProcessNode(CommandBuffer cmd)
        {
            if(!base.ProcessNode(cmd))
                return false;
            output = new Mat(1080, 1920, MatType.CV_8UC4);
            if (capture.Read(output))
            {
                GetPreview();
                return true;
            }

            preview = null;
            return false;
        }

        protected override void Disable()
        {
            capture.Dispose();
        }
    }
}