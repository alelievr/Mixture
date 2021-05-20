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

		// Store internally the resolved settings (contains only absolute values and not inherit.)
		[NonSerialized]
		internal MixtureSettings resolvedSettings;

		public MixtureSettings()
		{
			// By default we mirror the settings, but if we call resolve, the reference will change.
			resolvedSettings = this;
		}

		public void ResolveAndUpdate(MixtureNode node)
		{
			this.node = node;
			var graph = node.graph;

			if (resolvedSettings == this || resolvedSettings == null)
				resolvedSettings = new MixtureSettings();

			resolvedSettings.width = ResolveWidth(node, graph);
			resolvedSettings.height = ResolveHeight(node, graph);
			resolvedSettings.depth = ResolveDepth(node, graph);
			resolvedSettings.widthPercent = widthPercent;
			resolvedSettings.heightPercent = heightPercent;
			resolvedSettings.depthPercent = depthPercent;
			resolvedSettings.potSize = potSize;
			resolvedSettings.dimension = ResolveTextureDimension(node, graph);
			resolvedSettings.outputChannels = ResolveOutputChannels(node, graph);
			resolvedSettings.outputPrecision = ResolveOutputPrecision(node, graph);
			resolvedSettings.wrapMode = ResolveWrapMode(node, graph);
			resolvedSettings.filterMode = ResolveFilterMode(node, graph);
			resolvedSettings.doubleBuffered = doubleBuffered;
			resolvedSettings.refreshMode = refreshMode;

			int ResolveWidth(MixtureNode node, MixtureGraph graph)
			{
				switch(sizeMode)
				{
					default:
					case OutputSizeMode.InheritFromGraph:
						return graph.settings.width;
					case OutputSizeMode.InheritFromParent:
						if (node?.parentSettingsNode == null)
							return graph.settings.width;
						return ResolveWidth(node.parentSettingsNode, graph);
					case OutputSizeMode.InheritFromChild:
						if (node?.childSettingsNode == null)
							return graph.settings.width;
						return ResolveWidth(node.childSettingsNode, graph);
					case OutputSizeMode.Absolute:
						return node.settings.width;
					case OutputSizeMode.ScaleOfParent:
						// TODO:
						return (int)(graph.settings.width * widthPercent);
				}
			}

			int ResolveHeight(MixtureNode node, MixtureGraph graph)
			{
				switch(sizeMode)
				{
					default:
					case OutputSizeMode.InheritFromGraph:
						return graph.settings.height;
					case OutputSizeMode.InheritFromParent:
						if (node?.parentSettingsNode == null)
							return graph.settings.height;
						return ResolveWidth(node.parentSettingsNode, graph);
					case OutputSizeMode.InheritFromChild:
						if (node?.childSettingsNode == null)
							return graph.settings.height;
						return ResolveWidth(node.childSettingsNode, graph);
					case OutputSizeMode.Absolute:
						return node.settings.height;
					case OutputSizeMode.ScaleOfParent:
						// TODO:
						return (int)(graph.settings.height * heightPercent);
				}
			}

			int ResolveDepth(MixtureNode node, MixtureGraph graph)
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
						return ResolveDepth(node.parentSettingsNode, graph);
					case OutputSizeMode.InheritFromChild:
						if (node?.childSettingsNode == null)
							return graph.settings.depth;
						return ResolveDepth(node.childSettingsNode, graph);
					case OutputSizeMode.Absolute:
						return node.settings.depth;
					case OutputSizeMode.ScaleOfParent:
						// TODO:
						return (int)(graph.outputNode.settings.depth * widthPercent);
				}
			}

			OutputPrecision ResolveOutputPrecision(MixtureNode node, MixtureGraph graph)
			{
				switch (outputPrecision)
				{
					case OutputPrecision.InheritFromGraph:
						return graph.settings.outputPrecision;
					case OutputPrecision.InheritFromChild:
						if (node?.childSettingsNode == null)
							return graph.settings.outputPrecision;
						return ResolveOutputPrecision(node.childSettingsNode, graph);
					case OutputPrecision.InheritFromParent:
						if (node?.parentSettingsNode == null)
							return graph.settings.outputPrecision;
						return ResolveOutputPrecision(node.parentSettingsNode, graph);
					default:
						return node.settings.outputPrecision;
				}
			}

			OutputChannel ResolveOutputChannels(MixtureNode node, MixtureGraph graph)
			{
				switch (outputChannels)
				{
					case OutputChannel.InheritFromGraph:
						return graph.settings.outputChannels;
					case OutputChannel.InheritFromChild:
						if (node?.childSettingsNode == null)
							return graph.settings.outputChannels;
						return ResolveOutputChannels(node.childSettingsNode, graph);
					case OutputChannel.InheritFromParent:
						if (node?.parentSettingsNode == null)
							return graph.settings.outputChannels;
						return ResolveOutputChannels(node.parentSettingsNode, graph);
					default:
						return node.settings.outputChannels;
				}
			}

			OutputDimension ResolveTextureDimension(MixtureNode node, MixtureGraph graph)
			{
				// if this function is called from the output node and the dimension is default, then we set it to a default value
				switch (dimension)
				{
					case OutputDimension.InheritFromGraph:
						return graph.settings.dimension;
					case OutputDimension.InheritFromChild:
						if (node?.childSettingsNode == null)
							return graph.settings.dimension;
						// settings.node can be null :/
						return ResolveTextureDimension(node.childSettingsNode, graph);
					case OutputDimension.InheritFromParent:
						if (node?.parentSettingsNode == null)
							return graph.settings.dimension;
						return ResolveTextureDimension(node.parentSettingsNode, graph);
					default:
						return node.settings.dimension;
				}
			}

			OutputWrapMode ResolveWrapMode(MixtureNode node, MixtureGraph graph)
			{
				// if this function is called from the output node and the dimension is default, then we set it to a default value
				switch (wrapMode)
				{
					case OutputWrapMode.InheritFromGraph:
						return graph.settings.wrapMode;
					case OutputWrapMode.InheritFromChild:
						if (node?.childSettingsNode == null)
							return graph.settings.wrapMode;
						return ResolveWrapMode(node.childSettingsNode, graph);
					case OutputWrapMode.InheritFromParent:
						if (node?.parentSettingsNode == null)
							return graph.settings.wrapMode;
						return ResolveWrapMode(node.parentSettingsNode, graph);
					default:
						return node.settings.wrapMode;
				}
			}

			OutputFilterMode ResolveFilterMode(MixtureNode node, MixtureGraph graph)
			{
				// if this function is called from the output node and the dimension is default, then we set it to a default value
				switch (filterMode)
				{
					case OutputFilterMode.InheritFromGraph:
						return graph.settings.filterMode;
					case OutputFilterMode.InheritFromChild:
						if (node?.childSettingsNode == null)
							return graph.settings.filterMode;
						return ResolveFilterMode(node.childSettingsNode, graph);
					case OutputFilterMode.InheritFromParent:
						if (node?.parentSettingsNode == null)
							return graph.settings.filterMode;
						return ResolveFilterMode(node.parentSettingsNode, graph);
					default:
						return node.settings.filterMode;
				}
			}
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
			var p = GetResolvedPrecision(graph);
			return p == OutputPrecision.Half || p == OutputPrecision.Full;
		}
		
		public bool CanEdit(EditFlags flag) => (this.editFlags & flag) != 0;

		public GraphicsFormat GetGraphicsFormat(MixtureGraph graph)
			=> ConvertToGraphicsFormat(GetResolvedChannels(graph), GetResolvedPrecision(graph));

		public int GetResolvedWidth(MixtureGraph graph) => resolvedSettings.width;
		public int GetResolvedHeight(MixtureGraph graph) => resolvedSettings.height;
		public int GetResolvedDepth(MixtureGraph graph) => resolvedSettings.depth;
		public OutputPrecision GetResolvedPrecision(MixtureGraph graph) => resolvedSettings.outputPrecision;
		public OutputChannel GetResolvedChannels(MixtureGraph graph) => resolvedSettings.outputChannels;
		public TextureDimension GetTextureDimension(MixtureGraph graph) => (TextureDimension)resolvedSettings.dimension;
		public TextureWrapMode GetResolvedWrapMode(MixtureGraph graph) => (TextureWrapMode)resolvedSettings.wrapMode;
		public FilterMode GetResolvedFilterMode(MixtureGraph graph) => (FilterMode)resolvedSettings.filterMode;

		public bool NeedsUpdate(MixtureGraph graph, Texture t, bool checkFormat = true)
		{
			return (GetGraphicsFormat(graph) != t.graphicsFormat && checkFormat)
				|| GetResolvedWidth(graph) != t.width
				|| GetResolvedHeight(graph) != t.height
				|| GetResolvedFilterMode(graph) != t.filterMode
				|| GetResolvedWrapMode(graph) != t.wrapMode;
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