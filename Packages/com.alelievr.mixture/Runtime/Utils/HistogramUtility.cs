using UnityEngine;
using UnityEngine.Rendering;
using System;

namespace Mixture
{
    public enum HistogramMode
    {
        Luminance,
        Color,
    }

    public class HistogramData
    {
        // TODO: compute buffer, latest stats, mode, ect...
        public ComputeBuffer    histogram;
        public ComputeBuffer    histogramData;
        public HistogramMode    mode;
        public int              bucketCount;

        public Material         previewMaterial;
        public float            minLuminance;
        public float            maxLuminance;
    }

    public static class HistogramUtility
    {
        static ComputeShader _histogramCompute;
        static ComputeShader histogramCompute
        {
            get
            {
                if (_histogramCompute == null)
                    _histogramCompute = Resources.Load<ComputeShader>("Mixture/Histogram");
                return _histogramCompute;
            }
        }

        static int clearKernel = histogramCompute.FindKernel("Clear");
        static int computeLuminanceBufferKernel = histogramCompute.FindKernel("ComputeLuminanceBuffer");
        static int reduceLuminanceBufferKernel = histogramCompute.FindKernel("ReduceLuminanceBuffer");
        static int generateHistogramKernel = histogramCompute.FindKernel("GenerateHistogram");
        static int computeHistogramDataKernel = histogramCompute.FindKernel("ComputeHistogramData");

        // Temp buffer to store the luminance of the input image
        static ComputeBuffer _luminanceBuffer;
        static ComputeBuffer luminanceBuffer
        {
            get
            {
                if (_luminanceBuffer == null)
                    _luminanceBuffer = new ComputeBuffer(1048576, sizeof(float) * 2, ComputeBufferType.Structured);
                return _luminanceBuffer;
            }
        }

        static int _dataCount;

        public static void AllocateHistogramData(int histogramBucketCount, HistogramMode mode, out HistogramData data)
        {
            data = new HistogramData();
            data.bucketCount = histogramBucketCount;
            data.mode = mode;
			data.histogram = new ComputeBuffer(histogramBucketCount, sizeof(uint) * 4, ComputeBufferType.Structured);
            data.histogramData = new ComputeBuffer(1, sizeof(uint) * 2, ComputeBufferType.Structured);
            data.previewMaterial = new Material(Shader.Find("Hidden/HistogramPreview")) {hideFlags = HideFlags.HideAndDontSave};
            _dataCount++;
        }

        public static void ComputeHistogram(CommandBuffer cmd, Texture input, HistogramData data)
        {
            using (new ProfilingScope(cmd, new ProfilingSampler("Generate Histogram")))
            {
                // Clear buffers
                cmd.SetComputeBufferParam(histogramCompute, clearKernel, "_ImageLuminance", luminanceBuffer);
                cmd.SetComputeBufferParam(histogramCompute, clearKernel, "_Histogram", data.histogram);
                cmd.DispatchCompute(histogramCompute, clearKernel, input.width * input.height / 64, 1, 1);

                // Find luminance min / max in the texture
                // TODO: handle texture 3D and Cube
                cmd.SetComputeTextureParam(histogramCompute, computeLuminanceBufferKernel, "_Input", input);
                cmd.SetComputeBufferParam(histogramCompute, computeLuminanceBufferKernel, "_ImageLuminance", luminanceBuffer);
                cmd.SetComputeVectorParam(histogramCompute, "_InputTextureSize", new Vector4(input.width, input.height, TextureUtils.GetSliceCount(input), 0));
                cmd.DispatchCompute(histogramCompute, computeLuminanceBufferKernel, Mathf.Max(1, input.width / 8), Mathf.Max(1, input.height / 8), TextureUtils.GetSliceCount(input));

                // Reduce luminance buffer to find min/max
                cmd.SetComputeBufferParam(histogramCompute, reduceLuminanceBufferKernel, "_ImageLuminance", luminanceBuffer);
                cmd.SetComputeVectorParam(histogramCompute, "_InputTextureSize", new Vector4(input.width, input.height, TextureUtils.GetSliceCount(input), 0));
                cmd.DispatchCompute(histogramCompute, reduceLuminanceBufferKernel, 1, 1, 1);

                // Generate histogram data in compute buffer
                cmd.SetComputeBufferParam(histogramCompute, generateHistogramKernel, "_ImageLuminance", luminanceBuffer);
                cmd.SetComputeBufferParam(histogramCompute, generateHistogramKernel, "_Histogram", data.histogram);
                cmd.SetComputeTextureParam(histogramCompute, generateHistogramKernel, "_Input", input);
                cmd.SetComputeIntParam(histogramCompute, "_HistogramBucketCount", data.bucketCount);
                cmd.DispatchCompute(histogramCompute, generateHistogramKernel, Mathf.Max(1, input.width / 8), Mathf.Max(1, input.height / 8), TextureUtils.GetSliceCount(input));

                cmd.SetComputeBufferParam(histogramCompute, computeHistogramDataKernel, "_HistogramData", data.histogramData);
                cmd.SetComputeBufferParam(histogramCompute, computeHistogramDataKernel, "_Histogram", data.histogram);
                cmd.DispatchCompute(histogramCompute, computeHistogramDataKernel, Mathf.Max(1, data.bucketCount / 64), 1, 1);

                // Request histogram data back for inspector
                cmd.RequestAsyncReadback(luminanceBuffer, 8, 0, (c) => {
                    var d = c.GetData<float>();
                    if (d.Length > 0)
                    {
                        data.minLuminance = d[0];
                        data.maxLuminance = d[1];
                    }
                });
            }
        }

        public static void SetupHistogramPreviewMaterial(HistogramData data)
        {
            // previewMaterial.SetBuffer("_Histogram", histogramBuffer);
            data.previewMaterial.SetInt("_HistogramBucketCount", data.bucketCount);
            // data.previewMaterial.SetBuffer("_ImageLuminance", luminanceBuffer);
            // data.previewMaterial.SetBuffer("_Histogram", histogramBuffer);
            data.previewMaterial.SetBuffer("_HistogramReadOnly", data.histogram);
            data.previewMaterial.SetBuffer("_HistogramDataReadOnly", data.histogramData);
            data.previewMaterial.SetFloat("_Mode", (int)data.mode);
        }

        public static void Dispose(HistogramData data)
        {
            _dataCount--;

            data.histogram?.Dispose();
            data.histogramData?.Dispose();
            CoreUtils.Destroy(data.previewMaterial);

            if (_dataCount == 0)
                luminanceBuffer.Dispose();
        }
    }
}