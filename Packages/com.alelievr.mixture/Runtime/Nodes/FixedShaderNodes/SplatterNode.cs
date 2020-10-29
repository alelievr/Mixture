using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using UnityEngine.Rendering;

namespace Mixture
{
	[Documentation(@"
Distribute a set of input textures based on parameter-based patterns.
Most of the settings of this node are available in the inspector so don't hesitate to pin the node and tweak the parameters until you achieve your goal.

Note that when you connect multiple textures in the ""Splat Textures"" port, they will be randomly selected at each splat operation.
The limit of different input textures you can connect is 16, after new textures will be ignored.

When you generate the tiles, you can also choose to output the UVs of the tiles using the channel mode in the inspector, this can be useful to generate a noise based on these UVs.
")]
	[System.Serializable, NodeMenuItem("Textures/Splatter"), NodeMenuItem("Textures/Scatter")]
	public class SplatterNode : ComputeShaderNode
	{
		// Note: keep in sync with Splatter.compute switch case _Sequence
		public enum Sequence
		{
			Grid, Stack, R2, FibonacciSpiral,
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

		public enum OutputChannelMode
		{
			InputR, InputG, InputB, InputA,
			UV_X, UV_Y,
			RandomUniformColor,
		}

		public enum Mode
		{
			Sprites,
			DepthTile,
		}

		[Input]
		public List<Texture> inputTextures = new List<Texture>();

		[Output]
		public Texture output;

		[ShowInInspector, Range(1, 1820)] 
		public int maxSplatCount = 256;

		public Mode mode;

		public Sequence sequence;

		// Stack
		[VisibleIf(nameof(sequence), Sequence.Stack)]
		public Vector3 stackPosition = Vector3.zero;

		// Grid
		[VisibleIf(nameof(sequence), Sequence.Grid)]
		public Vector2 gridScale = new Vector2(8, 8);
		[VisibleIf(nameof(sequence), Sequence.Grid)]
		public Vector2 gridCram = Vector2.zero;
		[VisibleIf(nameof(sequence), Sequence.Grid)]
		public Vector2 gridShift = Vector2.zero;

		// R2
		[VisibleIf(nameof(sequence), Sequence.R2), Range(0, 1)]
		public float lambda = 0;

		// Fibonacci Spiral
		[VisibleIf(nameof(sequence), Sequence.FibonacciSpiral)]
		public float rotation;
		[VisibleIf(nameof(sequence), Sequence.FibonacciSpiral)]
		public float radius = 1;
		[VisibleIf(nameof(sequence), Sequence.FibonacciSpiral)]
		public float goldenRatio = 2.399999f;

		// Other
		[ShowInInspector]
		public Vector3 positionOffset = Vector3.zero;
		[ShowInInspector]
		public Vector3 positionJitter = Vector3.zero;

		// Rotation
		[ShowInInspector]
		public RotationMode rotationMode;
		[ShowInInspector, VisibleIf(nameof(rotationMode), RotationMode.Fixed)]
		public Vector3 fixedAngles;
		[ShowInInspector, VisibleIf(nameof(rotationMode), RotationMode.RandomBetween)]
		public Vector3 minAngles = new Vector3(-180, -180, -180);
		[ShowInInspector, VisibleIf(nameof(rotationMode), RotationMode.RandomBetween)]
		public Vector3 maxAngles = new Vector3(180, 180, 180);

		[ShowInInspector]
		public ScaleMode scaleMode = ScaleMode.Fixed;
		// Scale
		[ShowInInspector, VisibleIf(nameof(scaleMode), ScaleMode.Fixed)]
		public Vector3 fixedScale = Vector3.one;
		[ShowInInspector, VisibleIf(nameof(scaleMode), ScaleMode.RandomBetween)]
		public Vector3 minScale = new Vector3(0.5f, 0.5f, 0.5f);
		[ShowInInspector, VisibleIf(nameof(scaleMode), ScaleMode.RandomBetween)]
		public Vector3 maxScale = new Vector3(1.5f, 1.5f, 1.5f);

		[VisibleIf(nameof(mode), Mode.Sprites)]
		public Operator blendOperator = Operator.Blend;

		[Header("Output Channels")]

		// TODO: custom channel config
		// [ShowInInspector]
		// public OutputChannelMode channelModeR = OutputChannelMode.InputR;
		// [ShowInInspector]
		// public OutputChannelMode channelModeG = OutputChannelMode.InputG;
		// [ShowInInspector]
		// public OutputChannelMode channelModeB = OutputChannelMode.InputB;
		// [ShowInInspector]
		// public OutputChannelMode channelModeA = OutputChannelMode.InputA;

		[ShowInInspector, VisibleIf(nameof(mode), Mode.DepthTile)]
		public CompareFunction depthTest = CompareFunction.LessEqual;

		public override string name => "Splatter";
		protected override string computeShaderResourcePath => "Mixture/Splatter";
		public override List<OutputDimension> supportedDimensions => new List<OutputDimension>() {
			// TODO: support of Texture3D and cubemaps
			OutputDimension.Texture2D,
		};
		public override bool showDefaultInspector => true;
        protected override bool tempRenderTextureHasDepthBuffer => true;

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
		static readonly int _Lambda = Shader.PropertyToID("_Lambda");
		static readonly int _FibonacciRotation = Shader.PropertyToID("_FibonacciRotation");
		static readonly int _Radius = Shader.PropertyToID("_Radius");
		static readonly int _GoldenRatio = Shader.PropertyToID("_GoldenRatio");
		static readonly int _FixedAngles = Shader.PropertyToID("_FixedAngles");
		static readonly int _MinAngles = Shader.PropertyToID("_MinAngles");
		static readonly int _MaxAngles = Shader.PropertyToID("_MaxAngles");
		static readonly int _FixedScale = Shader.PropertyToID("_FixedScale");
		static readonly int _MinScale = Shader.PropertyToID("_MinScale");
		static readonly int _MaxScale = Shader.PropertyToID("_MaxScale");
		static readonly int _PositionJitter = Shader.PropertyToID("_PositionJitter");
		static readonly int _PositionOffset = Shader.PropertyToID("_PositionOffset");
		static readonly int _Time = Shader.PropertyToID("_Time");
		static readonly int _ElementCount = Shader.PropertyToID("_ElementCount");
		static readonly int _TextureCount = Shader.PropertyToID("_TextureCount");
		static readonly int _SrcBlend = Shader.PropertyToID("_SrcBlend");
		static readonly int _ZTest = Shader.PropertyToID("_ZTest");
		static readonly int _DstBlend = Shader.PropertyToID("_DstBlend");
		static readonly int _BlendOp = Shader.PropertyToID("_BlendOp");
		static readonly int _ChannelModeR = Shader.PropertyToID("_ChannelModeR");
		static readonly int _ChannelModeG = Shader.PropertyToID("_ChannelModeG");
		static readonly int _ChannelModeB = Shader.PropertyToID("_ChannelModeB");
		static readonly int _ChannelModeA = Shader.PropertyToID("_ChannelModeA");

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
			// // TODO: removing one edge from the list of custom port input set the list to null ! 
			// // FIXIT: do not reset the value if there is still something connected in the port
			// if (inputTextures == null)
			// 	inputTextures = new List<Texture>();

			// Create the list of input textures to splat
			inputTextures.Clear();
			var textureList = inputEdges.Select(e => e.passThroughBuffer as Texture).ToList();
			inputTextures.AddRange(textureList);

			if (inputTextures.Count > 16)
				Debug.LogError("Max number of splat input texture reached for splatter node, please remove one.");
		}

		[CustomPortBehavior(nameof(output))]
		protected IEnumerable< PortData > ChangeOutputPortType(List< SerializableEdge > edges)
		{
			yield return new PortData{
				displayName = "output",
				displayType = TextureUtils.GetTypeFromDimension(rtSettings.GetTextureDimension(graph)),
				identifier = "output",
				acceptMultipleEdges = true,
			};
		}

		protected override void Enable()
		{
			base.Enable();

			argumentBuffer = new ComputeBuffer(1, sizeof(int) * 5, ComputeBufferType.IndirectArguments);
			// We have a max of 16k splats, should be enough :) It means a total of 1820 particles max (we divide by 9 for tiling)
			splatPointsBuffer = new ComputeBuffer(16384, sizeof(float) * 9);

			generatePointKernel = computeShader.FindKernel("GenerateSplatPoints");
		}

		protected override bool ProcessNode(CommandBuffer cmd)
		{
			if (!base.ProcessNode(cmd))
				return false;
			
			int safeMaxSplatCount = Mathf.Min(sequence == Sequence.Grid ? Mathf.CeilToInt(gridScale.x * gridScale.y) : int.MaxValue, maxSplatCount);
			// TODO: remove me
			safeMaxSplatCount = maxSplatCount;

			SetComputeArgs(cmd);
			computeShader.GetKernelThreadGroupSizes(generatePointKernel, out uint x, out _, out _);
			DispatchCompute(cmd, generatePointKernel, safeMaxSplatCount + ((int)x - safeMaxSplatCount % (int)x));

			indirectArguments[0] = 6;
			indirectArguments[1] = Mathf.Max(0, safeMaxSplatCount * 9);
			indirectArguments[2] = 0;
			indirectArguments[3] = 0;
			indirectArguments[4] = 0;

			argumentBuffer.SetData(indirectArguments);

			var drawIndirectMat = GetTempMaterial("Hidden/Mixture/Splatter");

			// Set input textures:
			drawIndirectMat.SetInt(_TextureCount, inputTextures.Count);
			for (int i = 0; i < inputTextures.Count; i++)
				drawIndirectMat.SetTexture("_Source" + i + MixtureUtils.texture2DPrefix, inputTextures[i]);

			SetRenderStates(drawIndirectMat);

			drawIndirectMat.SetBuffer(_SplatPoints, splatPointsBuffer);
			cmd.SetRenderTarget(tempRenderTexture.colorBuffer, tempRenderTexture.depthBuffer);
			cmd.ClearRenderTarget(true, true, Color.clear);
			cmd.DrawProceduralIndirect(Matrix4x4.identity, drawIndirectMat, 0, MeshTopology.Triangles, argumentBuffer, 0);

			output = tempRenderTexture;

			return true;
		}

		void SetComputeArgs(CommandBuffer cmd)
		{
			cmd.SetComputeBufferParam(computeShader, generatePointKernel, _SplatPoints, splatPointsBuffer);
			cmd.SetComputeFloatParam(computeShader, _Time, (Application.isPlaying) ? Time.time : Time.realtimeSinceStartup);
			cmd.SetComputeFloatParam(computeShader, _ElementCount, maxSplatCount);
			cmd.SetComputeIntParam(computeShader, _Sequence, (int)sequence);
			cmd.SetComputeIntParam(computeShader, _RotationMode, (int)rotationMode);
			cmd.SetComputeIntParam(computeShader, _ScaleMode, (int)scaleMode);
			cmd.SetComputeVectorParam(computeShader, _StackPosition, stackPosition);
			cmd.SetComputeVectorParam(computeShader, _GridSize, gridScale);
			cmd.SetComputeVectorParam(computeShader, _GridCram, gridCram);
			cmd.SetComputeVectorParam(computeShader, _GridShift, gridShift);
			cmd.SetComputeFloatParam(computeShader, _Lambda, lambda);
			cmd.SetComputeFloatParam(computeShader, _FibonacciRotation, rotation * Mathf.Deg2Rad);
			cmd.SetComputeFloatParam(computeShader, _Radius, radius);
			cmd.SetComputeFloatParam(computeShader, _GoldenRatio, goldenRatio);
			cmd.SetComputeVectorParam(computeShader, _FixedAngles, fixedAngles);
			cmd.SetComputeVectorParam(computeShader, _MinAngles, minAngles);
			cmd.SetComputeVectorParam(computeShader, _MaxAngles, maxAngles);
			cmd.SetComputeVectorParam(computeShader, _FixedScale, fixedScale);
			cmd.SetComputeVectorParam(computeShader, _MinScale, minScale);
			cmd.SetComputeVectorParam(computeShader, _MaxScale, maxScale);
			cmd.SetComputeVectorParam(computeShader, _PositionJitter, positionJitter);
			cmd.SetComputeVectorParam(computeShader, _PositionOffset, positionOffset);
		}

		void SetRenderStates(Material mat)
		{
			switch (mode)
			{
				default:
				case Mode.Sprites:
					SetupBlendMode();
					SetupColorChannels();
					mat.SetFloat(_ZTest, (float)CompareFunction.Always);
					break;
				case Mode.DepthTile:
					SetupDepthChannels();
					mat.SetFloat(_SrcBlend, (float)BlendMode.One);
					mat.SetFloat(_DstBlend, (float)BlendMode.Zero);
					mat.SetFloat(_BlendOp, (float)BlendOp.Add);
					mat.SetFloat(_ZTest, (float)depthTest);
					break;
			}

			void SetupDepthChannels()
			{
				mat.SetFloat(_ChannelModeR, (int)OutputChannelMode.UV_X);
				mat.SetFloat(_ChannelModeG, (int)OutputChannelMode.UV_Y);
				mat.SetFloat(_ChannelModeB, (int)OutputChannelMode.RandomUniformColor);
				mat.SetFloat(_ChannelModeA, (int)OutputChannelMode.InputR);
			}

			void SetupColorChannels()
			{
				mat.SetFloat(_ChannelModeR, (int)OutputChannelMode.InputR);
				mat.SetFloat(_ChannelModeG, (int)OutputChannelMode.InputG);
				mat.SetFloat(_ChannelModeB, (int)OutputChannelMode.InputB);
				mat.SetFloat(_ChannelModeA, (int)OutputChannelMode.InputA);
			}

			void SetupBlendMode()
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
		}

		protected override void Disable()
		{
			argumentBuffer?.Dispose();
			splatPointsBuffer?.Dispose();
			base.Disable();
		}
	}
}