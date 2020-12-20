using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using GraphProcessor;
using System;
using Object = UnityEngine.Object;
using UnityEngine.Experimental.Rendering;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Mixture
{
	[System.Serializable]
	public struct MixtureRTSettings
	{
		[Range(0.0001f, 1.0f)]
		public float widthPercent;
		[Range(0.0001f, 1.0f)]
		public float heightPercent;
		[Range(0.0001f, 1.0f)]
		public float depthPercent;
		[Min(1)]
		public int width;
		[Min(1)]
		public int height;
		[Min(1)]
		public int sliceCount;
		public POTSize potSize;
		public OutputSizeMode widthMode;
		public OutputSizeMode heightMode;
		public OutputSizeMode depthMode;
		public OutputDimension dimension;
		public GraphicsFormat graphicsFormat => ConvertToGraphicsFormat(outputChannels, outputPrecision);
		public OutputChannel outputChannels;
		public OutputPrecision outputPrecision;
		public EditFlags editFlags;
		public bool doubleBuffered;
        public TextureWrapMode wrapMode;
        public FilterMode filterMode;
		public RefreshMode refreshMode;
		public float period;

		public OutputSizeMode sizeMode
		{
			get => widthMode;
			set
			{
				widthMode = value;
				heightMode = value;
				depthMode = value;
			}
		}

		public static MixtureRTSettings defaultValue
		{
			get => new MixtureRTSettings()
			{
				widthPercent = 1.0f,
				heightPercent = 1.0f,
				depthPercent = 1.0f,
				width = 1024,
				height = 1024,
				sliceCount = 1,
				widthMode = OutputSizeMode.Default,
				heightMode = OutputSizeMode.Default,
				depthMode = OutputSizeMode.Default,
				dimension = OutputDimension.SameAsOutput,
				outputChannels = OutputChannel.SameAsOutput,
				outputPrecision = OutputPrecision.SameAsOutput,
				editFlags = ~EditFlags.POTSize,
				doubleBuffered = false,
                wrapMode = TextureWrapMode.Repeat,
                filterMode = FilterMode.Bilinear,
				refreshMode = RefreshMode.OnLoad,
			};
		}

		internal static GraphicsFormat ConvertToGraphicsFormat(OutputChannel channels, OutputPrecision precisions)
		{
			return (channels, precisions) switch
			{
				// RGBA
				(OutputChannel.RGBA, OutputPrecision.LDR) => GraphicsFormat.R8G8B8A8_UNorm,
				(OutputChannel.RGBA, OutputPrecision.Half) => GraphicsFormat.R16G16B16A16_SFloat,
				(OutputChannel.RGBA, OutputPrecision.Full) => GraphicsFormat.R32G32B32A32_SFloat,

				// RG
				(OutputChannel.RG, OutputPrecision.LDR) => GraphicsFormat.R8G8_UNorm,
				(OutputChannel.RG, OutputPrecision.Half) => GraphicsFormat.R16G16_SFloat,
				(OutputChannel.RG, OutputPrecision.Full) => GraphicsFormat.R32G32_SFloat,

				// R
				(OutputChannel.R, OutputPrecision.LDR) => GraphicsFormat.R8_UNorm,
				(OutputChannel.R, OutputPrecision.Half) => GraphicsFormat.R16_SFloat,
				(OutputChannel.R, OutputPrecision.Full) => GraphicsFormat.R32_SFloat,

				// Conversion not found
				(var x, var y) => throw new Exception($"Missing GraphicsFormat conversion for {x} {y}"),
			};
		}

        public bool IsHDR(MixtureGraph graph)
		{
			var p = GetOutputPrecision(graph);
			return p == OutputPrecision.Half || p == OutputPrecision.Full;
		}
		
		public bool CanEdit(EditFlags flag) => (this.editFlags & flag) != 0;

		public int GetWidth(MixtureGraph graph)
		{
			switch(widthMode)
			{
				default:
				case OutputSizeMode.Default : return graph.outputNode.rtSettings.width;
				case OutputSizeMode.Fixed : return width;
				case OutputSizeMode.PercentageOfOutput : return (int)(graph.outputNode.rtSettings.width * widthPercent);
			}
		}

		public int GetHeight(MixtureGraph graph)
		{
			switch(heightMode)
			{
				default:
				case OutputSizeMode.Default : return graph.outputNode.rtSettings.height;
				case OutputSizeMode.Fixed : return height;
				case OutputSizeMode.PercentageOfOutput : return (int)(graph.outputNode.rtSettings.height * heightPercent);
			}
		}

		public int GetDepth(MixtureGraph graph)
		{
			var d = dimension == OutputDimension.SameAsOutput ? graph.outputNode.rtSettings.dimension : dimension;
			if (d == OutputDimension.Texture2D || d == OutputDimension.CubeMap)
				return 1;
			
			switch(depthMode)
			{
				default:
				case OutputSizeMode.Default : return graph.outputNode.rtSettings.sliceCount;
				case OutputSizeMode.Fixed : return sliceCount;
				case OutputSizeMode.PercentageOfOutput : return (int)(graph.outputNode.rtSettings.sliceCount * depthPercent);
			}
		}

		public GraphicsFormat GetGraphicsFormat(MixtureGraph graph) => ConvertToGraphicsFormat(GetOutputChannels(graph), GetOutputPrecision(graph));

		public OutputPrecision GetOutputPrecision(MixtureGraph graph) => outputPrecision == OutputPrecision.SameAsOutput ? graph.outputNode.rtSettings.outputPrecision : outputPrecision;
		public OutputChannel GetOutputChannels(MixtureGraph graph) => outputChannels == OutputChannel.SameAsOutput ? graph.outputNode.rtSettings.outputChannels : outputChannels;

		public TextureDimension GetTextureDimension(MixtureGraph graph)
		{
			// if this function is called from the output node and the dimension is default, then we set it to a default value
			if (graph?.outputNode == null || graph.outputNode.rtSettings.dimension == OutputDimension.SameAsOutput)
				return TextureDimension.Tex2D;
			else
				return dimension == OutputDimension.SameAsOutput ? (TextureDimension)graph.outputNode.rtSettings.dimension : (TextureDimension)dimension;
		}

		public bool NeedsUpdate(MixtureGraph graph, Texture t, bool checkFormat = true)
		{
			return (GetGraphicsFormat(graph) != t.graphicsFormat && checkFormat)
				|| GetWidth(graph) != t.width
				|| GetHeight(graph) != t.height
				|| filterMode != t.filterMode
				|| wrapMode != t.wrapMode;
		}

		public void SetPOTSize(int size)
		{
			potSize = (POTSize)Mathf.ClosestPowerOfTwo(size);
			width = height = sliceCount = (int)potSize;
		}
	}
}