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
		public OutputSizeMode widthMode;
		public OutputSizeMode heightMode;
		public OutputSizeMode depthMode;
		public OutputDimension dimension;
		public OutputFormat targetFormat;
		public EditFlags editFlags;
		public bool doubleBuffered;

		public static MixtureRTSettings defaultValue
		{
			get => new MixtureRTSettings()
			{
				widthPercent = 1.0f,
				heightPercent = 1.0f,
				depthPercent = 1.0f,
				width = 512,
				height = 512,
				sliceCount = 1,
				widthMode = OutputSizeMode.Default,
				heightMode = OutputSizeMode.Default,
				depthMode = OutputSizeMode.Default,
				dimension = OutputDimension.Default,
				targetFormat = OutputFormat.Default,
				editFlags = EditFlags.None,
				doubleBuffered = false,
			};
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
				case OutputSizeMode.PercentageOfOutput : return (int)(graph.outputNode.rtSettings.height * height);
			}
		}

		public int GetDepth(MixtureGraph graph)
		{
			switch(depthMode)
			{
				default:
				case OutputSizeMode.Default : return graph.outputNode.rtSettings.sliceCount;
				case OutputSizeMode.Fixed : return sliceCount;
				case OutputSizeMode.PercentageOfOutput : return (int)(graph.outputNode.rtSettings.sliceCount * depthPercent);
			}
		}

		public GraphicsFormat GetGraphicsFormat(MixtureGraph graph)
		{
			// if this function is called from the output node and the format is none, then we set it to a default value
			if (graph.outputNode.rtSettings.targetFormat == OutputFormat.Default)
				return (GraphicsFormat)OutputFormat.RGBA_Float;
			else
				return targetFormat == OutputFormat.Default ? (GraphicsFormat)graph.outputNode.rtSettings.targetFormat : (GraphicsFormat)targetFormat;
		}
		
		public TextureDimension GetTextureDimension(MixtureGraph graph)
		{
			// if this function is called from the output node and the dimension is default, then we set it to a default value
			if (graph.outputNode.rtSettings.dimension == OutputDimension.Default)
				return TextureDimension.Tex2D;
			else
				return dimension == OutputDimension.Default ? (TextureDimension)graph.outputNode.rtSettings.dimension : (TextureDimension)dimension;
		}
	}
}