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

		ComputeBuffer minMaxBuffer;
		internal ComputeBuffer histogram;
		float[] manualMinMaxData = new float[2];

		[CustomPortBehavior(nameof(input))]
		protected IEnumerable< PortData > ChangeInputPortType(List< SerializableEdge > edges)
		{
			yield return new PortData{
				displayName = "Input",
				displayType = TextureUtils.GetTypeFromDimension(rtSettings.GetTextureDimension(graph)),
				identifier = "Input",
				acceptMultipleEdges = false,
			};
		}

		[CustomPortBehavior(nameof(output))]
		protected IEnumerable< PortData > ChangeOutputPortType(List< SerializableEdge > edges)
		{
			yield return new PortData{
				displayName = "Output",
				displayType = TextureUtils.GetTypeFromDimension(rtSettings.GetTextureDimension(graph)),
				identifier = "output",
				acceptMultipleEdges = true,
			};
		}

        protected override void Enable()
        {
            base.Enable();

			findMinMaxKernel = computeShader.FindKernel("FindMinMax");
			levelsKernel = computeShader.FindKernel("Levels");
			previewKernel = computeShader.FindKernel("Preview");

			minMaxBuffer = new ComputeBuffer(2, sizeof(float), ComputeBufferType.Raw);
			histogram = new ComputeBuffer(histogramBucketCount, sizeof(uint), ComputeBufferType.Raw);
			UpdateTempRenderTexture(ref output);
        }

        protected override bool ProcessNode(CommandBuffer cmd)
        {
            if (!base.ProcessNode(cmd) || input == null)
				return false;

			UpdateTempRenderTexture(ref output);

			if (mode == Mode.Automatic)
			{
				cmd.SetComputeTextureParam(computeShader, findMinMaxKernel, "_Input", input);
				cmd.SetComputeBufferParam(computeShader, findMinMaxKernel, "_MinMax", minMaxBuffer);
				DispatchCompute(cmd, findMinMaxKernel, input.width, input.height, TextureUtils.GetSliceCount(input));
			}
			else
			{
				manualMinMaxData[0] = min;
				manualMinMaxData[1] = max;
				minMaxBuffer.SetData(manualMinMaxData);
			}

			uint[] zero = new uint[256];
			cmd.SetComputeBufferData(histogram, zero); // Nice but to optimize.
			cmd.SetComputeTextureParam(computeShader, levelsKernel, "_Input", input);
			cmd.SetComputeTextureParam(computeShader, levelsKernel, "_Output", output);
			cmd.SetComputeBufferParam(computeShader, levelsKernel, "_MinMax", minMaxBuffer);
			cmd.SetComputeBufferParam(computeShader, levelsKernel, "_Histogram", histogram);
			cmd.SetComputeIntParam(computeShader, "_HistogramBucketCount", histogramBucketCount);
			DispatchCompute(cmd, levelsKernel, input.width, input.height, TextureUtils.GetSliceCount(input));

			cmd.SetComputeTextureParam(computeShader, previewKernel, "_Output", output);
			DispatchComputePreview(cmd, previewKernel);

			return true;
        }

        protected override void Disable()
        {
            base.Disable();
			minMaxBuffer.Dispose();
			CoreUtils.Destroy(output);
        }

    
    
	}
}