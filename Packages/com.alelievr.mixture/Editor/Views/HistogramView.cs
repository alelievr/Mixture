using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
using UnityEditor;

namespace Mixture
{
	public class HistogramView : ImmediateModeElement 
	{
        Material        histogramPreview;
        ComputeBuffer   histogramBuffer;
        ComputeBuffer   histogramDataBuffer;
        ComputeBuffer   luminanceBuffer;
        ComputeShader   histogramCompute;

        int     histogramBucketCount;
        Texture inputTexture;

        int clearKernel;
        int computeLuminanceBufferKernel;
        int reduceLuminanceBufferKernel;
        int generateHistogramKernel;
        int computeHistogramDataKernel;

        public HistogramView(int height = 120, int histogramBucketCount = 256)
        {
			histogramPreview = new Material(Shader.Find("Hidden/HistogramPreview")) { hideFlags = HideFlags.HideAndDontSave };
            style.flexGrow = 1;
            style.height = height;
            this.histogramBucketCount = histogramBucketCount;

            histogramCompute = Resources.Load<ComputeShader>("Mixture/Histogram");
            clearKernel = histogramCompute.FindKernel("Clear");
            computeLuminanceBufferKernel = histogramCompute.FindKernel("ComputeLuminanceBuffer");
            reduceLuminanceBufferKernel = histogramCompute.FindKernel("ReduceLuminanceBuffer");
            generateHistogramKernel = histogramCompute.FindKernel("GenerateHistogram");
            computeHistogramDataKernel = histogramCompute.FindKernel("ComputeHistogramData");

            // DO NOT ALLOCATE COMPUTE BUFFERS HERE, IT DOESN'T WORK
            histogramDataBuffer = new ComputeBuffer(1, sizeof(uint) * 2, ComputeBufferType.Structured);
            histogramBuffer = new ComputeBuffer(histogramBucketCount, sizeof(uint) * 4, ComputeBufferType.Raw, ComputeBufferMode.Dynamic);
            luminanceBuffer = new ComputeBuffer(1048576, sizeof(float) * 2, ComputeBufferType.Structured);

            RegisterCallback<DetachFromPanelEvent>(Dispose);
        }

        public void UpdateHistogram(Texture input)
        {
            if (input == null)
                input = Texture2D.blackTexture;

            CommandBuffer cmd = new CommandBuffer() { name = "Update Histogram Texture" };
            inputTexture = input;

            // WORKAROUND!!! Unity throws errors in the console when the resources are not explicitely re-created
            // and Set through the ComputeShader API ... So we need to do this :(
            // Note that it's completely ignored and the true parameters are set via the command buffer
            // if (luminanceBuffer == null)
            //     // luminanceBuffer.Dispose();
            // // Store the luminance data of the image: max possible resolution: 8192x8192 with kernel of 8x8
            // luminanceBuffer = new ComputeBuffer(1048576, sizeof(float) * 2, ComputeBufferType.Structured);
            // if (histogramBuffer == null)
            //     // histogramBuffer.Dispose();
            // histogramBuffer = new ComputeBuffer(histogramBucketCount, sizeof(uint) * 4, ComputeBufferType.Raw);
            // if (histogramDataBuffer == null)
            //     // histogramDataBuffer.Dispose();
            // histogramDataBuffer = new ComputeBuffer(1, sizeof(uint) * 2, ComputeBufferType.Structured);

            // Clear buffers
            cmd.SetComputeBufferParam(histogramCompute, clearKernel, "_ImageLuminance", luminanceBuffer);
            cmd.SetComputeBufferParam(histogramCompute, clearKernel, "_Histogram", histogramBuffer);
            cmd.DispatchCompute(histogramCompute, clearKernel, histogramBucketCount / 64, 1, 1);

            // Find luminance min / max in the texture
            // TODO: handle texture 3D and Cube
            cmd.SetComputeTextureParam(histogramCompute, computeLuminanceBufferKernel, "_Input", input);
            cmd.SetComputeBufferParam(histogramCompute, computeLuminanceBufferKernel, "_ImageLuminance", luminanceBuffer);
            cmd.DispatchCompute(histogramCompute, computeLuminanceBufferKernel, Mathf.Max(1, input.width / 8), Mathf.Max(1, input.height / 8), TextureUtils.GetSliceCount(input));

            // Reduce luminance buffer to find min/max
            cmd.SetComputeBufferParam(histogramCompute, reduceLuminanceBufferKernel, "_ImageLuminance", luminanceBuffer);
            cmd.DispatchCompute(histogramCompute, reduceLuminanceBufferKernel, 1, 1, 1);

            // Generate histogram data in compute buffer
            cmd.SetComputeBufferParam(histogramCompute, generateHistogramKernel, "_ImageLuminance", luminanceBuffer);
            cmd.SetComputeBufferParam(histogramCompute, generateHistogramKernel, "_Histogram", histogramBuffer);
            cmd.SetComputeTextureParam(histogramCompute, generateHistogramKernel, "_Input", input);
            cmd.SetComputeIntParam(histogramCompute, "_HistogramBucketCount", histogramBucketCount); // TODO: constant
            cmd.DispatchCompute(histogramCompute, generateHistogramKernel, Mathf.Max(1, input.width / 8), Mathf.Max(1, input.height / 8), TextureUtils.GetSliceCount(input));


            cmd.SetComputeBufferParam(histogramCompute, computeHistogramDataKernel, "_HistogramData", histogramDataBuffer);
            cmd.SetComputeBufferParam(histogramCompute, computeHistogramDataKernel, "_Histogram", histogramBuffer);
            cmd.DispatchCompute(histogramCompute, computeHistogramDataKernel, Mathf.Max(1, histogramBucketCount / 64), 1, 1);

            Graphics.ExecuteCommandBuffer(cmd);
        }

        protected override void ImmediateRepaint()
        {
            // histogramPreview.SetBuffer("_Histogram", histogramBuffer);
            histogramPreview.SetInt("_HistogramBucketCount", histogramBucketCount);
            histogramPreview.SetBuffer("_ImageLuminance", luminanceBuffer);
            histogramPreview.SetBuffer("_Histogram2", histogramBuffer);
            histogramPreview.SetBuffer("_Histogram", histogramBuffer);
            histogramPreview.SetBuffer("_HistogramRW", histogramBuffer);
            histogramPreview.SetBuffer("_HistogramR", histogramBuffer);
            histogramPreview.SetBuffer("_HistogramData", histogramDataBuffer);
            var d = new float[2];
            luminanceBuffer.GetData(d, 0, 0, 2);
            // Debug.Log(d[0] + " | " + d[1]);
            // Debug.Log("Render with buffer: " + histogramBuffer.GetHashCode() + " | " + inputTexture);
            // Debug.Log("Render with buffer: " + luminanceBuffer.GetHashCode() + " | " + inputTexture);
            // Graphics.DrawTexture(contentRect, Texture2D.whiteTexture, histogramPreview, 0);
            EditorGUI.DrawPreviewTexture(contentRect, Texture2D.whiteTexture, histogramPreview);
            var style = new GUIStyle();
            style.normal.textColor = Color.red;
            GUI.Label(contentRect, "Hello world", style);
        }

        void Dispose(DetachFromPanelEvent e)
        {
            Debug.Log("DISPOSE?????");
            // histogramBuffer.Dispose();
            // luminanceBuffer.Dispose();
            // histogramDataBuffer.Dispose();
        }
	}
}