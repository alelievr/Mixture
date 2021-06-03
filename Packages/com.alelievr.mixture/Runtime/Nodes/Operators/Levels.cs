using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using UnityEngine.Rendering;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Operators/Levels")]
	public class Levels : ComputeShaderNode
	{
		public enum Mode
		{
			Manual,
			Automatic,
		}

		public enum ChannelMode
		{
			Single,
			Separate,
		}

		public Mode mode;

		[Input]
		public Texture input;

		[Input]
		public float min = 0;
		[Input]
		public float max = 1;

		[ShowInInspector]
		public ChannelMode channelMode = ChannelMode.Single;

		[ShowInInspector, VisibleIf(nameof(channelMode), ChannelMode.Single)]
		public AnimationCurve interpolationCurve = AnimationCurve.Linear(0, 0, 1, 1);

		[ShowInInspector, VisibleIf(nameof(channelMode), ChannelMode.Separate)]
		public AnimationCurve interpolationCurveR = AnimationCurve.Linear(0, 0, 1, 1);
		[ShowInInspector, VisibleIf(nameof(channelMode), ChannelMode.Separate)]
		public AnimationCurve interpolationCurveG = AnimationCurve.Linear(0, 0, 1, 1);
		[ShowInInspector, VisibleIf(nameof(channelMode), ChannelMode.Separate)]
		public AnimationCurve interpolationCurveB = AnimationCurve.Linear(0, 0, 1, 1);

		[SerializeField, HideInInspector]
		HistogramMode	histogramMode = HistogramMode.Luminance;

		[Output]
		public Texture output;

		public override string	name => "Levels";
		public override bool	showDefaultInspector => true;
		protected override string computeShaderResourcePath => "Mixture/Histogram";

        // In case you want to change the compute
        // protected override string previewKernel => null;
        // public override string previewTexturePropertyName => previewComputeProperty;

		[SerializeField, HideInInspector]
		Texture2D curveTexture;

		[SerializeField, HideInInspector]
		Texture2D curveTextureR;
		Texture2D curveTextureG;
		Texture2D curveTextureB;

		static internal readonly int histogramBucketCount = 256;

		internal ComputeBuffer minMaxBuffer;
		[SerializeField, HideInInspector]
		internal HistogramData histogramData;

		float[] minMaxBufferData = new float[2];

        protected override void Enable()
        {
            base.Enable();

			minMaxBuffer = new ComputeBuffer(1, sizeof(float) * 2, ComputeBufferType.Structured);
			HistogramUtility.AllocateHistogramData(histogramBucketCount, histogramMode, out histogramData);
        }

        protected override bool ProcessNode(CommandBuffer cmd)
        {
            if (!base.ProcessNode(cmd) || input == null)
				return false;

			HistogramUtility.ComputeLuminanceMinMax(cmd, minMaxBuffer, input);

			if (channelMode == ChannelMode.Single)
				TextureUtils.UpdateTextureFromCurve(interpolationCurve, ref curveTexture);
			else
			{
				TextureUtils.UpdateTextureFromCurve(interpolationCurveR, ref curveTextureR);
				TextureUtils.UpdateTextureFromCurve(interpolationCurveG, ref curveTextureG);
				TextureUtils.UpdateTextureFromCurve(interpolationCurveB, ref curveTextureB);
			}

			var mat = tempRenderTexture.material = GetTempMaterial("Hidden/Mixture/Levels");
			mat.SetFloat("_Mode", (int)mode);
			mat.SetInt("_ChannelMode", (int)channelMode);
			mat.SetFloat("_ManualMin", min);
			mat.SetFloat("_ManualMax", max);
			mat.SetVector("_RcpTextureSize", new Vector4(1.0f / input.width, 1.0f / input.height, 1.0f / TextureUtils.GetSliceCount(input), 0));
			MixtureUtils.SetupDimensionKeyword(mat, tempRenderTexture.dimension);
			MixtureUtils.SetTextureWithDimension(mat, "_Input", input);
			mat.SetBuffer("_Luminance", minMaxBuffer);
			mat.SetTexture("_InterpolationCurve", curveTexture);

			tempRenderTexture.Update();
			CustomTextureManager.UpdateCustomRenderTexture(cmd, tempRenderTexture);

			output = tempRenderTexture;

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