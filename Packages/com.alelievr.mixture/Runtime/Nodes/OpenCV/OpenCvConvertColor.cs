using GraphProcessor;
using OpenCvSharp;
using UnityEngine.Rendering;

namespace Mixture.OpenCV
{
    [System.Serializable, NodeMenuItem("OpenCV/Convert Color")]
    public class OpenCvConvertColor : OpenCVNode
    {
        public override bool showDefaultInspector => true;
        public override bool needsInspector => true;
        
        public ColorConversionCodes conversionCode = ColorConversionCodes.BGRA2GRAY;
        public MatType targetType = MatType.CV_8UC1;
        
        protected override bool ProcessNode(CommandBuffer cmd)
        {
            if(!base.ProcessNode(cmd) || input == null)
                return false;
            
            output = new Mat(input.Size(), targetType);
            Cv2.CvtColor(input, output, conversionCode);
            GetPreview();
            return true;
        }
        
    }
}