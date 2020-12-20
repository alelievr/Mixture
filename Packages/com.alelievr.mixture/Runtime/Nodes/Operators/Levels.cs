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

		[Output]
		public Texture output;

		public override string	name => "Levels";
		public override bool	showDefaultInspector => true;
		protected override string computeShaderResourcePath => "Mixture/Levels";

        // In case you want to change the compute
        // protected override string previewKernel => null;
        // public override string previewTexturePropertyName => previewComputeProperty;

		[SerializeField, HideInInspector]
		Texture2D curveTexture;

		int setMinMaxKernel;
		int levelsKernel;
		int previewKernel;

		static internal readonly int histogramBucketCount = 256;

		internal ComputeBuffer minMaxBuffer;
		internal HistogramData histogramData;

		float[] minMaxBufferData = new float[2];

        protected override void Enable()
        {
            base.Enable();

			levelsKernel = computeShader.FindKernel("Levels");
			previewKernel = computeShader.FindKernel("Preview");

			minMaxBuffer = new ComputeBuffer(1, sizeof(float) * 2, ComputeBufferType.Structured);
			HistogramUtility.AllocateHistogramData(histogramBucketCount, histogramMode, out histogramData);
        }

        protected override bool ProcessNode(CommandBuffer cmd)
        {
            if (!base.ProcessNode(cmd) || input == null)
				return false;

			output = tempRenderTexture;

			var dim = rtSettings.GetTextureDimension(graph);
			MixtureUtils.SetupComputeDimensionKeyword(computeShader, dim);

			HistogramUtility.ComputeLuminanceMinMax(cmd, minMaxBuffer, input);

			// Maybe a SetBuffer would have been better
			if (mode == Mode.Manual)
			{
				minMaxBufferData[0] = min;
				minMaxBufferData[1] = max;
				cmd.SetComputeBufferData(minMaxBuffer, minMaxBufferData);
			}

			TextureUtils.UpdateTextureFromCurve(interpolationCurve, ref curveTexture);

			cmd.SetComputeVectorParam(computeShader, "_RcpTextureSize", new Vector4(1.0f / input.width, 1.0f / input.height, 1.0f / TextureUtils.GetSliceCount(input), 0));
			cmd.SetComputeTextureParam(computeShader, levelsKernel, "_Input", input);
			cmd.SetComputeTextureParam(computeShader, levelsKernel, "_Output", tempRenderTexture);
			cmd.SetComputeBufferParam(computeShader, levelsKernel, "_Luminance", minMaxBuffer);
			cmd.SetComputeTextureParam(computeShader, levelsKernel, "_InterpolationCurve", curveTexture);
			DispatchCompute(cmd, levelsKernel, tempRenderTexture.width, tempRenderTexture.height, TextureUtils.GetSliceCount(tempRenderTexture));

			return true;
        }

        protected override void Disable()
        {
            base.Disable();
			HistogramUtility.Dispose(histogramData);
			minMaxBuffer?.Dispose();
        }
	}
}