using System.Collections.Generic;
using GraphProcessor;
using OpenCvSharp;
using UnityEngine;
using UnityEngine.Rendering;
using Rect = OpenCvSharp.Rect;

namespace Mixture.OpenCV
{
    [System.Serializable, NodeMenuItem("OpenCV/Flood Fill Random Color")]
    public class OpenCvFloodFill : OpenCVNode
    {
        public override bool showDefaultInspector => true;
        
        public float thresholdMin = 128;
        public float thresholdMax = 255;

        [Output] public Mat binaryMask;
        [Output] public List<OpenCvShapeData> shapes;
        
        protected override bool ProcessNode(CommandBuffer cmd)
        {
            if(!base.ProcessNode(cmd) || input == null)
                return false;
            output = new Mat(input.Size(), MatType.CV_8UC3);
            var gray = new Mat();
            Cv2.CvtColor(input, gray, ColorConversionCodes.BGRA2GRAY);
            gray.ConvertTo(gray, MatType.CV_8U);
            var binary = new Mat();
            Cv2.Threshold(gray, binary, thresholdMin, thresholdMax, ThresholdTypes.Binary);
            Mat labels = new Mat();
            //int numLabels = binary.ConnectedComponents(labels);
            Mat stats = new Mat();
            Mat centroids = new Mat();
            int labelCount = Cv2.ConnectedComponentsWithStats(binary, labels, stats, centroids);

            shapes = new List<OpenCvShapeData>();
            
            //Debug.Log(numLabels);
            //Vec3b[] colors = new Vec3b[numLabels];
            //for (int i = 0; i < numLabels; i++)
            //{
            //    colors[i] = new Vec3b(
            //        (byte)Random.Range(0, 256),
            //        (byte)Random.Range(0, 256),
            //        (byte)Random.Range(0, 256)
            //    );
            //}
            for (int y = 0; y < input.Rows; y++)
            {
                for (int x = 0; x < input.Cols; x++)
                {
                    //if(binary.At<int>(y, x) == 0)
                    //{
                    //    output.Set(y, x, new Vec3b((byte)0, (byte)0, (byte)0));
                    //    continue;
                    //}
                    int label = labels.At<int>(y, x);
                    if (label == 0) // Background
                        continue;
                    var left = stats.Get<int>(label, (int)ConnectedComponentsTypes.Left);
                    var top = stats.Get<int>(label, (int)ConnectedComponentsTypes.Top);
                    var width = stats.Get<int>(label, (int)ConnectedComponentsTypes.Width);
                    var height = stats.Get<int>(label, (int)ConnectedComponentsTypes.Height);
                    // COmpute UVs
                    float u = (x - left) / (float)width;
                    float v = (y - top) / (float)height;
                    
                    output.Set(y, x, new Vec3b(
                        0,
                        (byte)(255 * u),
                        (byte)(255 * v)
                    ));
                }
            }
            
            // Fill Shapes Data
            for (int i = 0; i < labelCount; i++)
            {
                var left = stats.Get<int>(i, (int)ConnectedComponentsTypes.Left);
                var top = stats.Get<int>(i, (int)ConnectedComponentsTypes.Top);
                var width = stats.Get<int>(i, (int)ConnectedComponentsTypes.Width);
                var height = stats.Get<int>(i, (int)ConnectedComponentsTypes.Height);
                var area = stats.Get<int>(i, (int)ConnectedComponentsTypes.Area);
                
                var centroidX = centroids.Get<double>(i, 0);
                var centroidY = centroids.Get<double>(i, 1);
                
                var data = new OpenCvShapeData
                {
                    boundingBox = new UnityEngine.Rect(left, top, width, height),
                    area = area,
                    center = new Vector2((float)centroidX, (float)centroidY),
                    id = i
                };
                shapes.Add(data);
            }
            
            for (int i = 0; i < labelCount; i++)
            {
                var left = stats.Get<int>(i, (int)ConnectedComponentsTypes.Left);
                var top = stats.Get<int>(i, (int)ConnectedComponentsTypes.Top);
                var width = stats.Get<int>(i, (int)ConnectedComponentsTypes.Width);
                var height = stats.Get<int>(i, (int)ConnectedComponentsTypes.Height);
                var area = stats.Get<int>(i, (int)ConnectedComponentsTypes.Area);
                var centroidX = centroids.Get<double>(i, 0);
                var centroidY = centroids.Get<double>(i, 1);
                var rect = new Rect(top, left, width, height);
                //Cv2.Rectangle(output, new Point(left, top), new Point(left + width, top + height), new Scalar(0, 255, 0), 1);
                Debug.Log(stats.Get<int>(i, (int)ConnectedComponentsTypes.Left));
            }
            binaryMask = binary;
            //output = gray;
            GetPreview();
            return true;

        }

        
    }
}