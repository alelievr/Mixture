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
		public float min = 0;
		[Input]
		public float max = 1;

		[ShowInInspector]
		public AnimationCurve interpolationCurve = AnimationCurve.Linear(0, 0, 1, 1);

		[SerializeField, HideInInspector]
		HistogramMode	histogramMode = HistogramMode.Luminance;

		// TODO: animation curve interpolation

		[Output]
		public CustomRenderTexture output;

		public override string	name => "Levels";
		public override bool	showDefaultInspector => true;
		protected override string computeShaderResourcePath => "Mixture/Levels";

        // In case you want to change the compute
        // protected override string previewKernel => null;
        // public override string previewTexturePropertyName => previewComputeProperty;

		[SerializeField, HideInInspector]
		Texture2D curveTexture;

		int findMinMaxKernel;
		int levelsKernel;
		int previewKernel;

		static internal readonly int histogramBucketCount = 256;

		internal ComputeBuffer minMaxBuffer;
		internal HistogramData histogramData;
		float[] manualMinMaxData = new float[2];

        protected override void Enable()
        {
            base.Enable();

			findMinMaxKernel = computeShader.FindKernel("FindMinMax");
			levelsKernel = computeShader.FindKernel("Levels");
			previewKernel = computeShader.FindKernel("Preview");

			minMaxBuffer = new ComputeBuffer(1, sizeof(float) * 2, ComputeBufferType.Default);
			HistogramUtility.AllocateHistogramData(histogramBucketCount, histogramMode, out histogramData);
			UpdateTempRenderTexture(ref output);
        }

        protected override bool ProcessNode(CommandBuffer cmd)
        {
            if (!base.ProcessNode(cmd) || input == null)
				return false;

			UpdateTempRenderTexture(ref output);


			// TODO: compute shader keywords

			// TODO: clear kernel
			float[] zero = new float[2];
			cmd.SetComputeBufferData(minMaxBuffer, zero);

			if (mode == Mode.Automatic)
			{
				// TODO: find min max or get it from histogram data
				// cmd.SetComputeTextureParam(computeShader, findMinMaxKernel, "_Input", input);
				// cmd.SetComputeBufferParam(computeShader, findMinMaxKernel, "_MinMax", minMaxBuffer);
				// DispatchCompute(cmd, findMinMaxKernel, Mathf.Max(8, input.width), Mathf.Max(8, input.height), TextureUtils.GetSliceCount(input));
				manualMinMaxData[0] = 0; // TODO: as float
				manualMinMaxData[1] = 1;
				minMaxBuffer.SetData<float>(manualMinMaxData.ToList());
			}
			else
			{
				manualMinMaxData[0] = min; // TODO: as float
				manualMinMaxData[1] = max;
				cmd.SetComputeFloatParam(computeShader, "_Min", min);
				cmd.SetComputeFloatParam(computeShader, "_Max", max);
				minMaxBuffer.SetData<float>(manualMinMaxData.ToList());
			}

			TextureUtils.UpdateTextureFromCurve(interpolationCurve, ref curveTexture);

			cmd.SetComputeTextureParam(computeShader, levelsKernel, "_Input", input);
			cmd.SetComputeTextureParam(computeShader, levelsKernel, "_Output", output);
			cmd.SetComputeBufferParam(computeShader, levelsKernel, "_MinMax", minMaxBuffer);
			cmd.SetComputeTextureParam(computeShader, levelsKernel, "_InterpolationCurve", curveTexture);
			DispatchCompute(cmd, levelsKernel, output.width, output.height, TextureUtils.GetSliceCount(output));

			cmd.SetComputeTextureParam(computeShader, previewKernel, "_Output", output);
			DispatchComputePreview(cmd, previewKernel);

			// Update the histogram view after the levels operation
			HistogramUtility.ComputeHistogram(cmd, output, histogramData);

			return true;
        }

        protected override void Disable()
        {
            base.Disable();
			minMaxBuffer?.Dispose();
			HistogramUtility.Dispose(histogramData);
			CoreUtils.Destroy(output);
        }
	}
}