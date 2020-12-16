using UnityEngine;
using UnityEngine.Rendering;

namespace Mixture
{
    public static class HistogramUtility
    {
        // Temp buffer to store the luminance of the input image
        static ComputeBuffer luminanceBuffer;

        public static void AllocateBuffers(int histogramBucketCount, out ComputeBuffer histogramBuffer, out ComputeBuffer histogramData)
        {
            histogramBuffer = null;
            histogramData = null;
        }

        public static void ComputeHistogram(CommandBuffer cmd, Texture input, ComputeBuffer histogramBuffer, ComputeBuffer histogramData)
        {

        }

        // TODO: histogram render API
    }
}