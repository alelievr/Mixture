using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using GraphProcessor;
using System;
using Object = UnityEngine.Object;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Mixture
{
	[System.Serializable]
	public class MixtureSettings
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
		[Min(1), FormerlySerializedAs("sliceCount")]
		public int depth;
		public POTSize potSize;
		[FormerlySerializedAs("widthMode")]
		public OutputSizeMode sizeMode;
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

		public static MixtureSettings defaultValue
		{
			get => new MixtureSettings()
			{
				widthPercent = 1.0f,
				heightPercent = 1.0f,
				depthPercent = 1.0f,
				width = 1024,
				height = 1024,
				depth = 1,
				sizeMode = OutputSizeMode.InheritFromParent,
				dimension = OutputDimension.InheritFromParent,
				outputChannels = OutputChannel.InheritFromParent,
				outputPrecision = OutputPrecision.InheritFromParent,
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
			switch(sizeMode)
			{
				default:
				case OutputSizeMode.InheritFromGraph:
					return graph.settings.width;
				case OutputSizeMode.InheritFromParent:
				case OutputSizeMode.InheritFromChild:
					// TODO
					return graph.settings.width;
				case OutputSizeMode.Absolute:
					return width;
				case OutputSizeMode.ScaleOfParent:
					// TODO:
					return (int)(graph.outputNode.settings.width * widthPercent);
			}
		}

		public int GetHeight(MixtureGraph graph)
		{
			switch(sizeMode)
			{
				default:
				case OutputSizeMode.InheritFromGraph:
					return graph.settings.height;
				case OutputSizeMode.InheritFromParent:
				case OutputSizeMode.InheritFromChild:
					// TODO
					return graph.settings.height;
				case OutputSizeMode.Absolute:
					return height;
				case OutputSizeMode.ScaleOfParent:
					// TODO:
					return (int)(graph.outputNode.settings.height * heightPercent);
			}
		}

		public int GetDepth(MixtureGraph graph)
		{
			var d = GetTextureDimension(graph); 
			if (d == TextureDimension.Tex2D || d == TextureDimension.Cube)
				return 1;

			switch(sizeMode)
			{
				default:
				case OutputSizeMode.InheritFromGraph:
					return graph.settings.depth;
				case OutputSizeMode.InheritFromParent:
				case OutputSizeMode.InheritFromChild:
					// TODO
					return graph.settings.depth;
				case OutputSizeMode.Absolute:
					return depth;
				case OutputSizeMode.ScaleOfParent:
					// TODO:
					return (int)(graph.outputNode.settings.depth * widthPercent);
			}
		}

		public GraphicsFormat GetGraphicsFormat(MixtureGraph graph)
			=>ConvertToGraphicsFormat(GetOutputChannels(graph), GetOutputPrecision(graph));

		public OutputPrecision GetOutputPrecision(MixtureGraph graph)
		{
			switch (outputPrecision)
			{
				case OutputPrecision.InheritFromGraph:
					return graph.settings.outputPrecision;
				case OutputPrecision.InheritFromChild:
				case OutputPrecision.InheritFromParent:
					// TODO!
					return graph.settings.outputPrecision;
				default:
					return outputPrecision;
			}
		}

		public OutputChannel GetOutputChannels(MixtureGraph graph)
		{
			switch (outputChannels)
			{
				case OutputChannel.InheritFromGraph:
					return graph.settings.outputChannels;
				case OutputChannel.InheritFromChild:
				case OutputChannel.InheritFromParent:
					// TODO!
					return graph.settings.outputChannels;
				default:
					return outputChannels;
			}
		}

		public TextureDimension GetTextureDimension(MixtureGraph graph)
		{
			// if this function is called from the output node and the dimension is default, then we set it to a default value
			switch (dimension)
			{
				case OutputDimension.InheritFromGraph:
					return (TextureDimension)graph.settings.dimension;
				case OutputDimension.InheritFromChild:
				case OutputDimension.InheritFromParent:
					// TODO!
					return (TextureDimension)graph.settings.dimension;
				default:
					return (TextureDimension)dimension;
			}
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
			width = height = depth = (int)potSize;
		}

		public MixtureSettings Clone()
		{
			return new MixtureSettings
			{
				widthPercent = widthPercent,
				heightPercent = heightPercent,
				depthPercent = depthPercent,
				width = width,
				height = height,
				depth = depth,
				potSize = potSize,
				sizeMode = sizeMode,
				dimension = dimension,
				outputChannels = outputChannels,
				outputPrecision = outputPrecision,
				editFlags = editFlags,
				doubleBuffered = doubleBuffered,
				wrapMode = wrapMode,
				filterMode = filterMode,
				refreshMode = refreshMode,
				period = period,
			};
		}
	}
}