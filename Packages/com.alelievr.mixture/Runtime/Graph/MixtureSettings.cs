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
        public const int k_MaxTextureResolution = 16384;

		[Range(0.0001f, 1.0f), FormerlySerializedAs("widthPercent")]
		public float widthScale = 1.0f;
		[Range(0.0001f, 1.0f), FormerlySerializedAs("heightPercent")]
		public float heightScale = 1.0f;
		[Range(0.0001f, 1.0f), FormerlySerializedAs("depthPercent")]
		public float depthScale = 1.0f;
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

		public static MixtureSettings defaultValue
		{
			get => new MixtureSettings()
			{
				widthScale = 1.0f,
				heightScale = 1.0f,
				depthScale = 1.0f,
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

		public MixtureSettings()
		{
			// By default we mirror the settings, but if we call resolve, the reference will change.
			resolvedSettings = this;
		}

		public void ResolveAndUpdate(MixtureNode node)
		{
			this.node = node;
			var graph = node.graph;

			// Fixup scale issue that wasn't catch by the migration:
			if (widthScale == 0)
				widthScale = 1;
			if (heightScale == 0)
				heightScale = 1;
			if (depthScale == 0)
				depthScale = 1;

			if (resolvedSettings == this || resolvedSettings == null)
				resolvedSettings = new MixtureSettings();

			resolvedSettings.dimension = ResolveTextureDimension(node, graph);
			resolvedSettings.width = Mathf.Clamp(ResolveWidth(node, graph), 1, k_MaxTextureResolution);
			resolvedSettings.height = Mathf.Clamp(ResolveHeight(node, graph), 1, k_MaxTextureResolution);
			resolvedSettings.depth = Mathf.Clamp(ResolveDepth(node, graph), 1, k_MaxTextureResolution);
			resolvedSettings.widthScale = widthScale;
			resolvedSettings.heightScale = heightScale;
			resolvedSettings.depthScale = depthScale;
			resolvedSettings.potSize = potSize;
			resolvedSettings.outputChannels = ResolveOutputChannels(node, graph);
			resolvedSettings.outputPrecision = ResolveOutputPrecision(node, graph);
			resolvedSettings.wrapMode = ResolveWrapMode(node, graph);
			resolvedSettings.filterMode = ResolveFilterMode(node, graph);
			resolvedSettings.doubleBuffered = doubleBuffered;
			resolvedSettings.refreshMode = refreshMode;

			int ResolveWidth(MixtureNode node, MixtureGraph graph)
			{
				switch(node.settings.sizeMode)
				{
					default:
					case OutputSizeMode.InheritFromGraph:
						return (int)(graph.settings.width * node.settings.widthScale);
					case OutputSizeMode.InheritFromParent:
						if (node?.parentSettingsNode == null)
							return (int)(graph.settings.width * node.settings.widthScale);
						return (int)(ResolveWidth(node.parentSettingsNode, graph) * node.settings.widthScale);
					case OutputSizeMode.InheritFromChild:
						if (node?.childSettingsNode == null)
							return (int)(graph.settings.width * node.settings.widthScale);
						return (int)(ResolveWidth(node.childSettingsNode, graph) * node.settings.widthScale);
					case OutputSizeMode.Absolute:
						return node.settings.width;
				}
			}

			int ResolveHeight(MixtureNode node, MixtureGraph graph)
			{
				switch(node.settings.sizeMode)
				{
					default:
					case OutputSizeMode.InheritFromGraph:
						return (int)(graph.settings.height * node.settings.heightScale);
					case OutputSizeMode.InheritFromParent:
						if (node?.parentSettingsNode == null)
							return (int)(graph.settings.height * node.settings.heightScale);
						return (int)(ResolveWidth(node.parentSettingsNode, graph) * node.settings.heightScale);
					case OutputSizeMode.InheritFromChild:
						if (node?.childSettingsNode == null)
							return (int)(graph.settings.height * node.settings.heightScale);
						return (int)(ResolveWidth(node.childSettingsNode, graph) * node.settings.heightScale);
					case OutputSizeMode.Absolute:
						return node.settings.height;
				}
			}

			int ResolveDepth(MixtureNode node, MixtureGraph graph)
			{
				if (resolvedSettings.dimension != OutputDimension.Texture3D)
					return 1;

				switch(node.settings.sizeMode)
				{
					default:
					case OutputSizeMode.InheritFromGraph:
						return (int)(graph.settings.depth * node.settings.depthScale);
					case OutputSizeMode.InheritFromParent:
						if (node?.parentSettingsNode == null)
							return (int)(graph.settings.depth * node.settings.depthScale);
						return (int)(ResolveDepth(node.parentSettingsNode, graph) * node.settings.depthScale);
					case OutputSizeMode.InheritFromChild:
						if (node?.childSettingsNode == null)
							return (int)(graph.settings.depth * node.settings.depthScale);
						return (int)(ResolveDepth(node.childSettingsNode, graph) * node.settings.depthScale);
					case OutputSizeMode.Absolute:
						return node.settings.depth;
				}
			}

			OutputPrecision ResolveOutputPrecision(MixtureNode node, MixtureGraph graph)
			{
				switch (node.settings.outputPrecision)
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
				switch (node.settings.outputChannels)
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
				switch (node.settings.dimension)
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
				switch (node.settings.wrapMode)
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
				switch (node.settings.filterMode)
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

		public float GetUpdatePeriod()
		{
			switch (refreshMode)
			{
				case RefreshMode.EveryXFrame:
					return (1.0f / Application.targetFrameRate) * period;
				case RefreshMode.EveryXMillis:
					return period / 1000.0f;
				case RefreshMode.EveryXSeconds:
					return period;
				default:
				case RefreshMode.OnLoad:
					return 0;
			}
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
		public TextureDimension GetResolvedTextureDimension(MixtureGraph graph) => (TextureDimension)resolvedSettings.dimension;
		public TextureWrapMode GetResolvedWrapMode(MixtureGraph graph) => (TextureWrapMode)resolvedSettings.wrapMode;
		public FilterMode GetResolvedFilterMode(MixtureGraph graph) => (FilterMode)resolvedSettings.filterMode;

		public bool NeedsUpdate(MixtureGraph graph, Texture t, bool checkFormat = true)
		{
			return (checkFormat && GetGraphicsFormat(graph) != t.graphicsFormat)
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
				widthScale = widthScale,
				heightScale = heightScale,
				depthScale = depthScale,
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