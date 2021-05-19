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
        public OutputWrapMode wrapMode;
        public OutputFilterMode filterMode;
		public RefreshMode refreshMode;
		public float period;

		[NonSerialized]
		MixtureNode node;

		public void ResolveAndUpdate(MixtureNode node)
		{
			this.node = node;
			// TODO: update all cached values in the settings
		}

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
                wrapMode = OutputWrapMode.InheritFromParent,
                filterMode = OutputFilterMode.InheritFromParent,
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

		public void Initialize(MixtureGraph graph)
		{
		}

		public int GetResolvedWidth(MixtureGraph graph)
		{
			switch(sizeMode)
			{
				default:
				case OutputSizeMode.InheritFromGraph:
					return graph.settings.width;
				case OutputSizeMode.InheritFromParent:
					if (node?.parentSettingsNode == null)
						return graph.settings.width;
					return node.parentSettingsNode.settings.GetResolvedWidth(graph);
				case OutputSizeMode.InheritFromChild:
					if (node?.childSettingsNode == null)
						return graph.settings.width;
					return node.childSettingsNode.settings.GetResolvedWidth(graph);
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
					if (node?.parentSettingsNode == null)
						return graph.settings.height;
					return node.parentSettingsNode.settings.GetHeight(graph);
				case OutputSizeMode.InheritFromChild:
					if (node?.childSettingsNode == null)
						return graph.settings.height;
					return node.childSettingsNode.settings.GetHeight(graph);
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
					if (node?.parentSettingsNode == null)
						return graph.settings.depth;
					return node.parentSettingsNode.settings.GetDepth(graph);
				case OutputSizeMode.InheritFromChild:
					if (node?.childSettingsNode == null)
						return graph.settings.depth;
					return node.childSettingsNode.settings.GetDepth(graph);
				case OutputSizeMode.Absolute:
					return depth;
				case OutputSizeMode.ScaleOfParent:
					// TODO:
					return (int)(graph.outputNode.settings.depth * widthPercent);
			}
		}

		public GraphicsFormat GetGraphicsFormat(MixtureGraph graph)
			=> ConvertToGraphicsFormat(GetOutputChannels(graph), GetOutputPrecision(graph));

		public OutputPrecision GetOutputPrecision(MixtureGraph graph)
		{
			switch (outputPrecision)
			{
				case OutputPrecision.InheritFromGraph:
					return graph.settings.outputPrecision;
				case OutputPrecision.InheritFromChild:
					if (node?.childSettingsNode == null)
						return graph.settings.outputPrecision;
					return node.childSettingsNode.settings.GetOutputPrecision(graph);
				case OutputPrecision.InheritFromParent:
					if (node?.parentSettingsNode == null)
						return graph.settings.outputPrecision;
					return node.parentSettingsNode.settings.GetOutputPrecision(graph);
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
					if (node?.childSettingsNode == null)
						return graph.settings.outputChannels;
					return node.childSettingsNode.settings.GetOutputChannels(graph);
				case OutputChannel.InheritFromParent:
					if (node?.parentSettingsNode == null)
						return graph.settings.outputChannels;
					return node.parentSettingsNode.settings.GetOutputChannels(graph);
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
					if (node?.childSettingsNode == null)
						return (TextureDimension)graph.settings.dimension;
					return node.childSettingsNode.settings.GetTextureDimension(graph);
				case OutputDimension.InheritFromParent:
					if (node?.parentSettingsNode == null)
						return (TextureDimension)graph.settings.dimension;
					return node.parentSettingsNode.settings.GetTextureDimension(graph);
				default:
					return (TextureDimension)dimension;
			}
		}

		public TextureWrapMode GetWrapMode(MixtureGraph graph)
		{
			// if this function is called from the output node and the dimension is default, then we set it to a default value
			switch (wrapMode)
			{
				case OutputWrapMode.InheritFromGraph:
					return (TextureWrapMode)graph.settings.wrapMode;
				case OutputWrapMode.InheritFromChild:
					if (node?.childSettingsNode == null)
						return (TextureWrapMode)graph.settings.wrapMode;
					return node.childSettingsNode.settings.GetWrapMode(graph);
				case OutputWrapMode.InheritFromParent:
					if (node?.parentSettingsNode == null)
						return (TextureWrapMode)graph.settings.wrapMode;
					return node.parentSettingsNode.settings.GetWrapMode(graph);
				default:
					return (TextureWrapMode)wrapMode;
			}
		}

		public FilterMode GetFilterMode(MixtureGraph graph)
		{
			// if this function is called from the output node and the dimension is default, then we set it to a default value
			switch (filterMode)
			{
				case OutputFilterMode.InheritFromGraph:
					return (FilterMode)graph.settings.filterMode;
				case OutputFilterMode.InheritFromChild:
					if (node?.childSettingsNode == null)
						return (FilterMode)graph.settings.filterMode;
					return node.childSettingsNode.settings.GetFilterMode(graph);
				case OutputFilterMode.InheritFromParent:
					if (node?.parentSettingsNode == null)
						return (FilterMode)graph.settings.filterMode;
					return node.parentSettingsNode.settings.GetFilterMode(graph);
				default:
					return (FilterMode)filterMode;
			}
		}

		public bool NeedsUpdate(MixtureGraph graph, Texture t, bool checkFormat = true)
		{
			return (GetGraphicsFormat(graph) != t.graphicsFormat && checkFormat)
				|| GetResolvedWidth(graph) != t.width
				|| GetHeight(graph) != t.height
				|| GetFilterMode(graph) != t.filterMode
				|| GetWrapMode(graph) != t.wrapMode;
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

		public void SyncInheritanceMode(NodeInheritanceMode mode)
		{
			if (outputChannels.Inherits())
				outputChannels = (OutputChannel)mode;
			if (outputPrecision.Inherits())
				outputPrecision = (OutputPrecision)mode;
			if (dimension.Inherits())
				dimension = (OutputDimension)mode;
			if (wrapMode.Inherits())
				wrapMode = (OutputWrapMode)mode;
			if (filterMode.Inherits())
				filterMode = (OutputFilterMode)mode;
			if (sizeMode.Inherits())
				sizeMode = (OutputSizeMode)mode;
		}
	}
}