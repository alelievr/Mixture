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
	public abstract class MixtureNode : BaseNode
	{
		protected new MixtureGraph	graph => base.graph as MixtureGraph;

		protected void AddObjectToGraph(Object obj) => graph.AddObjectToGraph(obj);
		protected void RemoveObjectFromGraph(Object obj) => graph.RemoveObjectFromGraph(obj);

		public MixtureRTSettings rtSettings;

		public abstract Texture previewTexture { get; }

		public virtual MixtureRTSettings defaultRTSettings { get { return MixtureRTSettings.defaultValue; } }

		public MixtureNode() : base()
		{
			rtSettings = defaultRTSettings;
		}

		protected bool UpdateTempRenderTexture(ref RenderTexture target)
		{
			int width = rtSettings.GetWidth(graph);
			int height = rtSettings.GetHeight(graph);
			int depth = rtSettings.GetDepth(graph);
			GraphicsFormat targetFormat = rtSettings.GetGraphicsFormat(graph);
			TextureDimension dimension = rtSettings.GetTextureDimension(graph);

			if (target == null)
			{

				RenderTextureDescriptor	desc = new RenderTextureDescriptor {
					width = Math.Max(1, width),
					height = Math.Max(1, height),
					depthBufferBits = 0,
					volumeDepth = Math.Max(1,depth),
					dimension = dimension,
					graphicsFormat = targetFormat,
					msaaSamples = 1,
				};
				target = new RenderTexture(desc);
				target.name = $"Mixture Temp {name}";
				return true;
			}

			if (target.width != width
				|| target.height != height
				|| target.graphicsFormat != targetFormat
				|| target.dimension != dimension
				|| target.volumeDepth != depth
				|| target.filterMode != graph.outputTexture.filterMode)
			{
				target.Release();
				target.width = Math.Max(1, width);
				target.height = Math.Max(1, height);
				target.graphicsFormat = (GraphicsFormat)targetFormat;
				target.dimension = (TextureDimension)dimension;
				target.filterMode = graph.outputTexture.filterMode; // TODO Set FilterMode per-node, add FilterMode to RTSettings
				target.volumeDepth = depth;
				target.Create();
			}

			return false;
		}

#if UNITY_EDITOR
		protected Type GetPropertyType(MaterialProperty prop)
		{
			switch (prop.type)
			{
				case MaterialProperty.PropType.Color:
					return typeof(Color);
				case MaterialProperty.PropType.Float:
				case MaterialProperty.PropType.Range:
					return typeof(float);
				case MaterialProperty.PropType.Texture:
					return TextureUtils.GetTypeFromDimension(prop.textureDimension);
				default:
				case MaterialProperty.PropType.Vector:
					return typeof(Vector4);
			}
		}

		protected IEnumerable< PortData > GetMaterialPortDatas(Material material)
		{
			if (material == null)
				yield break;

			foreach (var prop in MaterialEditor.GetMaterialProperties(new []{material}))
			{
				if (prop.flags == MaterialProperty.PropFlags.HideInInspector
					|| prop.flags == MaterialProperty.PropFlags.NonModifiableTextureData
					|| prop.flags == MaterialProperty.PropFlags.PerRendererData)
					continue;

				yield return new PortData{
					identifier = prop.name,
					displayName = prop.displayName,
					displayType = GetPropertyType(prop),
				};
			}
		}

		protected void AssignMaterialPropertiesFromEdges(List< SerializableEdge > edges, Material material)
		{
			// Update material settings when processing the graph:
			foreach (var edge in edges)
			{
				var prop = MaterialEditor.GetMaterialProperty(new []{material}, edge.inputPort.portData.identifier);

				switch (prop.type)
				{
					case MaterialProperty.PropType.Color:
						prop.colorValue = (Color)edge.passThroughBuffer;
						break;
					case MaterialProperty.PropType.Texture:
						// TODO: texture scale and offset
						prop.textureValue = (Texture)edge.passThroughBuffer;
						break;
					case MaterialProperty.PropType.Float:
					case MaterialProperty.PropType.Range:
						prop.floatValue = (float)edge.passThroughBuffer;
						break;
					case MaterialProperty.PropType.Vector:
						prop.vectorValue = (Vector4)edge.passThroughBuffer;
						break;
				}
			}
		}
#endif
	}

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
		public int depth;
		public OutputSizeMode widthMode;
		public OutputSizeMode heightMode;
		public OutputSizeMode depthMode;
		public OutputDimension dimension;
		public OutputFormat targetFormat;
		public EditFlags editFlags;

		public static MixtureRTSettings defaultValue
		{
			get
			{
				return new MixtureRTSettings()
				{
					widthPercent = 1.0f,
					heightPercent = 1.0f,
					depthPercent = 1.0f,
					width = 512,
					height = 512,
					depth = 1,
					widthMode = OutputSizeMode.Default,
					heightMode = OutputSizeMode.Default,
					depthMode = OutputSizeMode.Default,
					dimension = OutputDimension.Default,
					targetFormat = OutputFormat.Default,
					editFlags = EditFlags.None
				};
			}
	
		}
		public bool CanEdit(EditFlags flag)
		{
			return (this.editFlags & flag) != 0;
		}

		public int GetWidth(MixtureGraph graph)
		{
			switch(widthMode)
			{
				default:
				case OutputSizeMode.Default : return graph.outputNode.tempRenderTexture.width;
				case OutputSizeMode.Fixed : return width;
				case OutputSizeMode.PercentageOfOutput : return (int)(graph.outputNode.tempRenderTexture.width * widthPercent);
			}
		}

		public int GetHeight(MixtureGraph graph)
		{
			switch(heightMode)
			{
				default:
				case OutputSizeMode.Default : return graph.outputNode.tempRenderTexture.height;
				case OutputSizeMode.Fixed : return height;
				case OutputSizeMode.PercentageOfOutput : return (int)(graph.outputNode.tempRenderTexture.height * height);
			}
		}

		public int GetDepth(MixtureGraph graph)
		{
			switch(depthMode)
			{
				default:
				case OutputSizeMode.Default : return graph.outputNode.sliceCount;
				case OutputSizeMode.Fixed : return depth;
				case OutputSizeMode.PercentageOfOutput : return (int)(graph.outputNode.sliceCount * depthPercent);
			}
		}

		public GraphicsFormat GetGraphicsFormat(MixtureGraph graph)
		{
			return targetFormat == OutputFormat.Default ? graph.outputNode.tempRenderTexture.graphicsFormat : (GraphicsFormat)targetFormat;
		}
		
		public TextureDimension GetTextureDimension(MixtureGraph graph)
		{
			return dimension == OutputDimension.Default ? graph.outputNode.tempRenderTexture.dimension : (TextureDimension)dimension;
		}
	}

	public enum EditFlags
	{
		None = 0,
		Width = 1,
		WidthMode = 2,
		Height = 4,
		HeightMode = 8,
		Depth = 16,
		DepthMode = 32,
		Dimension = 64,
		TargetFormat = 128,
		All = 255,
	}

	public enum OutputSizeMode
	{
		Default = 0,
		Fixed = 1,
		PercentageOfOutput = 2
	}


	public enum OutputDimension
	{
		Default = TextureDimension.None,
		Texture2D = TextureDimension.Tex2D,
		CubeMap = TextureDimension.Cube,
		Texture3D = TextureDimension.Tex3D,
		Texture2DArray = TextureDimension.Tex2DArray,
		CubemapArray = TextureDimension.CubeArray
	}

	public enum OutputFormat
	{
		Default = GraphicsFormat.None,
		RGBA_LDR = GraphicsFormat.R8G8B8A8_UNorm,
		RGB_LDR = GraphicsFormat.R8G8B8_UNorm,
		RGBA_Half = GraphicsFormat.R16G16B16A16_SFloat,
		RGB_Half = GraphicsFormat.R16G16B16_SFloat,
		RGBA_Float = GraphicsFormat.R32G32B32A32_SFloat,
		RGB_Float = GraphicsFormat.R32G32B32_SFloat,

	}
}