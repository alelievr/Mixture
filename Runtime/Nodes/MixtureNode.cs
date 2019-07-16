﻿using System.Collections.Generic;
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

		public virtual MixtureRTSettings defaultRTSettings => MixtureRTSettings.defaultValue;

		public abstract Texture previewTexture { get; }

		public event Action onSettingsChanged;

		public MixtureNode() : base() {}
		
		public override void OnNodeCreated()
		{
			base.OnNodeCreated();
			rtSettings = defaultRTSettings;
		}

		protected bool UpdateTempRenderTexture(ref RenderTexture target)
		{
			if (graph.outputTexture == null)
				return false;

			int width = rtSettings.GetWidth(graph);
			int height = rtSettings.GetHeight(graph);
			int depth = rtSettings.GetDepth(graph);
			GraphicsFormat targetFormat = rtSettings.GetGraphicsFormat(graph);
			TextureDimension dimension = rtSettings.GetTextureDimension(graph);

            if (targetFormat == GraphicsFormat.None)
                targetFormat = graph.outputTexture.graphicsFormat;

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

		public void OnSettingsChanged() => onSettingsChanged?.Invoke();
#endif
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
		// CubeMap = TextureDimension.Cube, // Not supported currently
		Texture3D = TextureDimension.Tex3D, // Not supported currently
		Texture2DArray = TextureDimension.Tex2DArray,
		// CubemapArray = TextureDimension.CubeArray // Not supported currently
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