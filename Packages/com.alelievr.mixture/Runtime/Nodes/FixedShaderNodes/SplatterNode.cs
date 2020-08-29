using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using UnityEngine.Rendering;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Textures/Splatter"), NodeMenuItem("Textures/Scatter")]
	public class SplatterNode : ComputeShaderNode
	{
		public enum Sequence
		{
			Stack, Grid, R2, Halton, FibonacciSpiral,
		}

		public enum RotationMode
		{
			Fixed, RandomBetween, TowardsCenter
		}

		public enum ScaleMode
		{
			Fixed, RandomBetween,
		}

		public enum Operator
		{
			Blend, PreMultiplied, Additive, SoftAdditive, Substractive, Multiplicative, Max, Min,
		}

		[Input]
		public List<Texture> inputTextures = new List<Texture>();

		[Output]
		public Texture output;

		public int maxSplatCount = 256;

		public Vector2 extent = Vector2.one;

		public Sequence sequence;

		// Stack
		[VisibleIf(nameof(sequence), Sequence.Stack)]
		public Vector3 stackPosition = Vector3.zero;

		// Grid
		[VisibleIf(nameof(sequence), Sequence.Grid)]
		public Vector2 gridSize = new Vector2(8, 8);
		[VisibleIf(nameof(sequence), Sequence.Grid)]
		public Vector2 gridCram = Vector2.zero;
		[VisibleIf(nameof(sequence), Sequence.Grid)]
		public Vector2 gridShift = Vector2.zero;

		// Other
		public Vector3 positionJitter = Vector3.zero;

		// R2
		public RotationMode rotationMode;

		// Rotation
		[VisibleIf(nameof(rotationMode), RotationMode.Fixed)]
		public Vector3 fixedAngles;
		[VisibleIf(nameof(rotationMode), RotationMode.RandomBetween)]
		public Vector3 minAngles = new Vector3(-180, -180, -180);
		[VisibleIf(nameof(rotationMode), RotationMode.RandomBetween)]
		public Vector3 maxAngles = new Vector3(180, 180, 180);

		public ScaleMode scaleMode = ScaleMode.Fixed;
		// Scale
		[VisibleIf(nameof(scaleMode), ScaleMode.Fixed)]
		public Vector3 fixedScale = Vector3.one;
		[VisibleIf(nameof(scaleMode), ScaleMode.RandomBetween)]
		public Vector3 minScale = new Vector3(0.5f, 0.5f, 0.5f);
		[VisibleIf(nameof(scaleMode), ScaleMode.RandomBetween)]
		public Vector3 maxScale = new Vector3(1.5f, 1.5f, 1.5f);

		public Operator blendOperator = Operator.Blend;

		public override string name => "Splatter";
		protected override string computeShaderResourcePath => "Mixture/Splatter";
		public override List<OutputDimension> supportedDimensions => new List<OutputDimension>() {
			// TODO: support of Texture3D
			OutputDimension.Texture2D,
		};
		public override bool showDefaultInspector => true;

		ComputeBuffer			argumentBuffer;
		ComputeBuffer			splatPointsBuffer;

		int		generatePointKernel;
		int		previewKernel;
		int[]	indirectArguments = new int[5];

		static readonly int	_Sequence = Shader.PropertyToID("_Sequence");
		static readonly int	_RotationMode = Shader.PropertyToID("_RotationMode");
		static readonly int	_ScaleMode = Shader.PropertyToID("_ScaleMode");
		static readonly int _SplatPoints = Shader.PropertyToID("_SplatPoints");
		static readonly int _StackPosition = Shader.PropertyToID("_StackPosition");
		static readonly int _GridSize = Shader.PropertyToID("_GridSize");
		static readonly int _GridCram = Shader.PropertyToID("_GridCram");
		static readonly int _GridShift = Shader.PropertyToID("_GridShift");
		static readonly int _FixedAngles = Shader.PropertyToID("_FixedAngles");
		static readonly int _MinAngles = Shader.PropertyToID("_MinAngles");
		static readonly int _MaxAngles = Shader.PropertyToID("_MaxAngles");
		static readonly int _FixedScale = Shader.PropertyToID("_FixedScale");
		static readonly int _MinScale = Shader.PropertyToID("_MinScale");
		static readonly int _MaxScale = Shader.PropertyToID("_MaxScale");
		static readonly int _PositionJitter = Shader.PropertyToID("_PositionJitter");
		static readonly int _Time = Shader.PropertyToID("_Time");
		static readonly int _ElementCount = Shader.PropertyToID("_ElementCount");
		static readonly int _Extent = Shader.PropertyToID("_Extent");
		static readonly int _TextureCount = Shader.PropertyToID("_TextureCount");
		static readonly int _SrcBlend = Shader.PropertyToID("_SrcBlend");
		static readonly int _DstBlend = Shader.PropertyToID("_DstBlend");
		static readonly int _BlendOp = Shader.PropertyToID("_BlendOp");

		[CustomPortBehavior(nameof(inputTextures))]
		IEnumerable<PortData> CustomInputTexturePortData(List<SerializableEdge> edges)
		{
			yield return new PortData
			{
				identifier = nameof(inputTextures),
				displayName = "Splat textures",
				displayType = typeof(Texture2D),
				acceptMultipleEdges = true,
			};
		}

		[CustomPortInput(nameof(inputTextures), typeof(Texture))]
		void PullInputs(List< SerializableEdge > inputEdges)
		{
			// TODO: removing one edge from the list of cusotm port input set the lsit to null ! 
			// FIXIT: do not reset the value if there is still something connected in the port
			if (inputTextures == null)
				inputTextures = new List<Texture>();

			// Create the list of input textures to splat
			inputTextures.Clear();
			var textureList = inputEdges.Select(e => e.passThroughBuffer as Texture).ToList();
			inputTextures.AddRange(textureList);

			if (inputTextures.Count > 16)
				Debug.LogError("Max number of splat input texture reached for splatter node, please remove one.");
		}

		protected override void Enable()
		{
			base.Enable();

			argumentBuffer = new ComputeBuffer(1, sizeof(int) * 5, ComputeBufferType.IndirectArguments);
			// We have a max of 16k splats, should be enough :)
			splatPointsBuffer = new ComputeBuffer(16384, sizeof(float) * 9);

			generatePointKernel = computeShader.FindKernel("GenerateSplatPoints");
		}

		protected override bool ProcessNode(CommandBuffer cmd)
		{
			if (!base.ProcessNode(cmd))
				return false;

			SetComputeArgs(cmd);
			computeShader.GetKernelThreadGroupSizes(generatePointKernel, out uint x, out _, out _);
			DispatchCompute(cmd, generatePointKernel, maxSplatCount + ((int)x - maxSplatCount % (int)x));
		
			indirectArguments[0] = 6;
			indirectArguments[1] = Mathf.Max(0, maxSplatCount);
			indirectArguments[2] = 0;
			indirectArguments[3] = 0;
			indirectArguments[4] = 0;

			argumentBuffer.SetData(indirectArguments);

			var drawIndirectMat = GetTempMaterial("Hidden/Mixture/Splatter");

			// Set input textures:
			drawIndirectMat.SetInt(_TextureCount, inputTextures.Count);
			for (int i = 0; i < inputTextures.Count; i++)
				drawIndirectMat.SetTexture("_Source" + i + "_2D", inputTextures[i]);

			SetBlendSettings(drawIndirectMat);

			drawIndirectMat.SetBuffer(_SplatPoints, splatPointsBuffer);
			cmd.SetRenderTarget(tempRenderTexture);
			cmd.ClearRenderTarget(false, true, Color.clear);
			cmd.DrawProceduralIndirect(Matrix4x4.identity, drawIndirectMat, 0, MeshTopology.Triangles, argumentBuffer, 0);

			output = tempRenderTexture;

			return true;
		}

		void SetComputeArgs(CommandBuffer cmd)
		{
			cmd.SetComputeBufferParam(computeShader, generatePointKernel, _SplatPoints, splatPointsBuffer);
			cmd.SetComputeFloatParam(computeShader, _Time, Time.time);
			cmd.SetComputeFloatParam(computeShader, _ElementCount, maxSplatCount);
			cmd.SetComputeIntParam(computeShader, _Sequence, (int)sequence);
			cmd.SetComputeIntParam(computeShader, _RotationMode, (int)rotationMode);
			cmd.SetComputeIntParam(computeShader, _ScaleMode, (int)scaleMode);
			cmd.SetComputeVectorParam(computeShader, _StackPosition, stackPosition);
			cmd.SetComputeVectorParam(computeShader, _GridSize, gridSize);
			cmd.SetComputeVectorParam(computeShader, _GridCram, gridCram);
			cmd.SetComputeVectorParam(computeShader, _GridShift, gridShift);
			cmd.SetComputeVectorParam(computeShader, _FixedAngles, fixedAngles);
			cmd.SetComputeVectorParam(computeShader, _MinAngles, minAngles);
			cmd.SetComputeVectorParam(computeShader, _MaxAngles, maxAngles);
			cmd.SetComputeVectorParam(computeShader, _FixedScale, fixedScale);
			cmd.SetComputeVectorParam(computeShader, _MinScale, minScale);
			cmd.SetComputeVectorParam(computeShader, _MaxScale, maxScale);
			cmd.SetComputeVectorParam(computeShader, _PositionJitter, positionJitter);
			cmd.SetComputeVectorParam(computeShader, _Extent, extent);
		}

		void SetBlendSettings(Material mat)
		{
			switch (blendOperator)
			{
				default:
				case Operator.Blend:
					mat.SetFloat(_SrcBlend, (int)BlendMode.SrcAlpha);
					mat.SetFloat(_DstBlend, (int)BlendMode.OneMinusSrcAlpha);
					mat.SetFloat(_BlendOp, (int)BlendOp.Add);
					break;
				case Operator.PreMultiplied:
					mat.SetFloat(_SrcBlend, (int)BlendMode.One);
					mat.SetFloat(_DstBlend, (int)BlendMode.OneMinusSrcAlpha);
					mat.SetFloat(_BlendOp, (int)BlendOp.Add);
					break;
				case Operator.Additive:
					mat.SetFloat(_SrcBlend, (int)BlendMode.One);
					mat.SetFloat(_DstBlend, (int)BlendMode.One);
					mat.SetFloat(_BlendOp, (int)BlendOp.Add);
					break;
				case Operator.SoftAdditive:
					mat.SetFloat(_SrcBlend, (int)BlendMode.OneMinusDstColor);
					mat.SetFloat(_DstBlend, (int)BlendMode.One);
					mat.SetFloat(_BlendOp, (int)BlendOp.Add);
					break;
				case Operator.Substractive:
					mat.SetFloat(_SrcBlend, (int)BlendMode.One);
					mat.SetFloat(_DstBlend, (int)BlendMode.One);
					mat.SetFloat(_BlendOp, (int)BlendOp.Subtract);
					break;
				case Operator.Multiplicative:
					mat.SetFloat(_SrcBlend, (int)BlendMode.DstColor);
					mat.SetFloat(_DstBlend, (int)BlendMode.Zero);
					mat.SetFloat(_BlendOp, (int)BlendOp.Add);
					break;
				case Operator.Max:
					mat.SetFloat(_SrcBlend, (int)BlendMode.One);
					mat.SetFloat(_DstBlend, (int)BlendMode.One);
					mat.SetFloat(_BlendOp, (int)BlendOp.Max);
					break;
				case Operator.Min:
					mat.SetFloat(_SrcBlend, (int)BlendMode.One);
					mat.SetFloat(_DstBlend, (int)BlendMode.One);
					mat.SetFloat(_BlendOp, (int)BlendOp.Min);
					break;
			}
		}

		protected override void Disable()
		{
			argumentBuffer?.Dispose();
			splatPointsBuffer?.Dispose();
			base.Disable();
		}
	}
}