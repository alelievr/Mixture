using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using GraphProcessor;
using System;
using System.Linq;
using Object = UnityEngine.Object;
using UnityEngine.Experimental.Rendering;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Rendering;
#endif

namespace Mixture
{
	public abstract class MixtureNode : BaseNode
	{
		protected new MixtureGraph			graph => base.graph as MixtureGraph;

		public MixtureRTSettings			rtSettings;

		protected virtual MixtureRTSettings	defaultRTSettings => MixtureRTSettings.defaultValue;
		public virtual float   				nodeWidth => 250.0f;
		public virtual Texture				previewTexture => null;
		public virtual bool					hasSettings => true;
		public virtual List<OutputDimension> supportedDimensions => new List<OutputDimension>() {
			OutputDimension.Texture2D,
			OutputDimension.Texture3D,
			OutputDimension.CubeMap,
		};

		public event Action					onSettingsChanged;
		
		public override void OnNodeCreated()
		{
			base.OnNodeCreated();
			rtSettings = defaultRTSettings;
		}

		protected bool UpdateTempRenderTexture(ref CustomRenderTexture target)
		{
			if (graph.outputTexture == null)
				return false;

			int outputWidth = rtSettings.GetWidth(graph);
			int outputHeight = rtSettings.GetHeight(graph);
			int outputDepth = rtSettings.GetDepth(graph);
			GraphicsFormat targetFormat = rtSettings.GetGraphicsFormat(graph);
			TextureDimension dimension = rtSettings.GetTextureDimension(graph);

            if (targetFormat == GraphicsFormat.None)
                targetFormat = graph.outputTexture.graphicsFormat;
			if (dimension == TextureDimension.None)
				dimension = TextureDimension.Tex2D;

			if (target == null)
			{
				target = new CustomRenderTexture(outputWidth, outputHeight, targetFormat)
				{
					volumeDepth = Math.Max(1, outputDepth),
					dimension = dimension,
					name = $"Mixture Temp {name}",
					updateMode = CustomRenderTextureUpdateMode.OnDemand,
				};
				target.Create();

				return true;
			}

			// TODO: check if format is supported by current system

			// Warning: here we use directly the settings from the 
			if (target.width != outputWidth
				|| target.height != outputHeight
				|| target.graphicsFormat != targetFormat
				|| target.dimension != dimension
				|| target.volumeDepth != outputDepth
				|| target.filterMode != graph.outputTexture.filterMode)
			{
				target.Release();
				target.width = Math.Max(1, outputWidth);
				target.height = Math.Max(1, outputHeight);
				target.graphicsFormat = (GraphicsFormat)targetFormat;
				target.dimension = (TextureDimension)dimension;
				target.filterMode = graph.outputTexture.filterMode; // TODO Set FilterMode per-node, add FilterMode to RTSettings
				target.volumeDepth = outputDepth;
				target.Create();
			}

			return false;
		}

		protected void AddObjectToGraph(Object obj) => graph.AddObjectToGraph(obj);
		protected void RemoveObjectFromGraph(Object obj) => graph.RemoveObjectFromGraph(obj);

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

			var currentDimension = rtSettings.GetTextureDimension(graph);
				
			foreach (var prop in MaterialEditor.GetMaterialProperties(new []{material}))
			{
				if (prop.flags == MaterialProperty.PropFlags.HideInInspector
					|| prop.flags == MaterialProperty.PropFlags.NonModifiableTextureData
					|| prop.flags == MaterialProperty.PropFlags.PerRendererData)
					continue;
				
				if (!PropertySupportsDimension(prop, currentDimension))
					continue;

				yield return new PortData{
					identifier = prop.name,
					displayName = prop.displayName,
					displayType = GetPropertyType(prop),
				};
			}
		}

		bool PropertySupportsDimension(MaterialProperty prop, TextureDimension dim)
		{
			return MixtureUtils.GetAllowedDimenions(prop.name).Contains(dim);
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

		protected bool IsShaderCompiled(Shader s)
		{
			return !ShaderUtil.GetShaderMessages(s).Any(m => m.severity == ShaderCompilerMessageSeverity.Error);
		}

		protected void LogShaderErrors(Shader s)
		{
			foreach (var m in ShaderUtil.GetShaderMessages(s).Where(m => m.severity == ShaderCompilerMessageSeverity.Error))
			{
				string file = String.IsNullOrEmpty(m.file) ? s.name : m.file;
				Debug.LogError($"{file}:{m.line} {m.message} {m.messageDetails}");
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
		CubeMap = TextureDimension.Cube,
		Texture3D = TextureDimension.Tex3D,
		// Texture2DArray = TextureDimension.Tex2DArray, // Not supported by CRT, will be handled as Texture3D and then saved as Tex2DArray
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