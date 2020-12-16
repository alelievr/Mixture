using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using UnityEngine.Rendering;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Compute/Levels")]
	public class Levels : ComputeShaderNode
	{
		public enum Mode
		{
			Manual,
			Automatic,
		}

		public Mode mode;

		[Input]
		public Texture input;

		[Input]
		public float min;
		[Input]
		public float max;

		// TODO: animation curve interpolation

		[Output]
		CustomRenderTexture output;

		public override string name => "Levels";

		protected override string computeShaderResourcePath => "Mixture/Levels";

        // In case you want to change the compute
        // protected override string previewKernel => null;
        // public override string previewTexturePropertyName => previewComputeProperty;

		int findMinMaxKernel;
		int levelsKernel;
		int previewKernel;

		static readonly int histogramBucketCount = 256;

		internal ComputeBuffer minMaxBuffer;
		internal ComputeBuffer histogram;
		float[] manualMinMaxData = new float[4];

        protected override void Enable()
        {
            base.Enable();

			findMinMaxKernel = computeShader.FindKernel("FindMinMax");
			levelsKernel = computeShader.FindKernel("Levels");
			previewKernel = computeShader.FindKernel("Preview");

			minMaxBuffer = new ComputeBuffer(16, sizeof(float), ComputeBufferType.Raw);
			histogram = new ComputeBuffer(histogramBucketCount, sizeof(uint), ComputeBufferType.Raw);
			UpdateTempRenderTexture(ref output);
        }

        protected override bool ProcessNode(CommandBuffer cmd)
        {
            if (!base.ProcessNode(cmd) || input == null)
				return false;

			UpdateTempRenderTexture(ref output);

			// HistogramUtility.ComputeHistogram()

			// TODO: compute shader keywords

			uint[] zero = new uint[256];
			uint[] zero2 = new uint[16];
			cmd.SetComputeBufferData(histogram, zero); // Nice but to optimize.
			cmd.SetComputeBufferData(minMaxBuffer, zero2);

			if (mode == Mode.Automatic)
			{
				cmd.SetComputeTextureParam(computeShader, findMinMaxKernel, "_Input", input);
				cmd.SetComputeBufferParam(computeShader, findMinMaxKernel, "_MinMax", minMaxBuffer);
				DispatchCompute(cmd, findMinMaxKernel, Mathf.Max(8, input.width), Mathf.Max(8, input.height), TextureUtils.GetSliceCount(input));
			}
			else
			{
				manualMinMaxData[0] = min; // TODO: as float
				manualMinMaxData[1] = max;
				minMaxBuffer.SetData(manualMinMaxData);
			}

			cmd.SetComputeTextureParam(computeShader, levelsKernel, "_Input", input);
			cmd.SetComputeTextureParam(computeShader, levelsKernel, "_Output", output);
			cmd.SetComputeBufferParam(computeShader, levelsKernel, "_MinMax", minMaxBuffer);
			cmd.SetComputeBufferParam(computeShader, levelsKernel, "_Histogram", histogram);
			cmd.SetComputeIntParam(computeShader, "_HistogramBucketCount", histogramBucketCount);
			DispatchCompute(cmd, levelsKernel, output.width, output.height, TextureUtils.GetSliceCount(output));

			cmd.SetComputeTextureParam(computeShader, previewKernel, "_Output", output);
			DispatchComputePreview(cmd, previewKernel);

			return true;
        }

        protected override void Disable()
        {
            base.Disable();
			minMaxBuffer.Dispose();
			histogram.Dispose();
			CoreUtils.Destroy(output);
        }
	}
}