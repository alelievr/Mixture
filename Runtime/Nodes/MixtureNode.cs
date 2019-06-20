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

		protected bool UpdateTempRenderTexture(ref RenderTexture target, GraphicsFormat targetFormat = GraphicsFormat.None)
		{
            if (targetFormat == GraphicsFormat.None)
                targetFormat = graph.outputTexture.graphicsFormat;

			if (target == null)
			{

				RenderTextureDescriptor	desc = new RenderTextureDescriptor {
					width = graph.outputTexture.width,
					height = graph.outputTexture.height,
					depthBufferBits = 0,
					volumeDepth = graph.outputNode.sliceCount,
					dimension = graph.outputTexture.dimension,
					graphicsFormat = targetFormat,
					msaaSamples = 1,
				};
				target = new RenderTexture(desc);
				target.name = $"Mixture Temp {name}";
				return true;
			}

			if (target.width != graph.outputTexture.width
				|| target.height != graph.outputTexture.height
				|| target.graphicsFormat != targetFormat
				|| target.dimension != graph.outputTexture.dimension
				|| target.volumeDepth != TextureUtils.GetSliceCount(graph.outputTexture)
				|| target.filterMode != graph.outputTexture.filterMode)
			{
				target.Release();
				target.width = graph.outputTexture.width;
				target.height = graph.outputTexture.height;
				target.graphicsFormat = targetFormat;
				target.dimension = graph.outputTexture.dimension;
				target.filterMode = graph.outputTexture.filterMode;
				target.volumeDepth = TextureUtils.GetSliceCount(graph.outputTexture);
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
}