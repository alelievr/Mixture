﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using GraphProcessor;
using System;
using System.Linq;
using Object = UnityEngine.Object;
using UnityEngine.Experimental.Rendering;
using System.Text.RegularExpressions;
using System.Reflection;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Rendering;
#endif
using UnityEngine.Profiling;

namespace Mixture
{
	public abstract class MixtureNode : BaseNode
	{
		public new MixtureGraph			graph => base.graph as MixtureGraph;

		[HideInInspector, FormerlySerializedAs("rtSettings")]
		public MixtureSettings				settings = MixtureSettings.defaultValue;
		[HideInInspector, Obsolete]
		public MixtureSettings				rtSettings;

		protected virtual MixtureSettings	defaultSettings => MixtureSettings.defaultValue;
		public virtual float   				nodeWidth => MixtureUtils.defaultNodeWidth;
		public virtual Texture				previewTexture => null;
		public virtual bool					hasSettings => true;
		public virtual bool					hasPreview => true;
		public virtual List<OutputDimension> supportedDimensions => new List<OutputDimension>() {
			OutputDimension.Texture2D,
			OutputDimension.Texture3D,
			OutputDimension.CubeMap,
		};
		public virtual PreviewChannels		defaultPreviewChannels => PreviewChannels.RGBA;

		public virtual bool					canEditPreviewSRGB => true;
		public virtual bool					defaultPreviewSRGB => false;

		public virtual bool					showDefaultInspector => false;
		public virtual bool					showPreviewExposure => false;
		[SerializeField, HideInInspector]
		public bool							isPreviewCollapsed = false;

		public event Action					onSettingsChanged;
		public event Action					beforeProcessSetup;
		public event Action					afterProcessCleanup;

		internal event Action				onEnabled;

		public override bool				showControlsOnHover => false; // Disable this feature for now

		public override bool				needsInspector => true;

		protected Dictionary<string, Material> temporaryMaterials = new Dictionary<string, Material>();

		// UI Serialization
		[SerializeField, HideInInspector]
		public PreviewChannels previewMode;
		[SerializeField, HideInInspector]
		public bool previewSRGB;

		[SerializeField, HideInInspector]
        public float previewMip = 0.0f;
        [SerializeField, HideInInspector]
        public bool previewVisible = true;
		[SerializeField, HideInInspector]
		public float previewEV100 = 0.0f;
		[HideInInspector]
		public float previewSlice = 0;
		[HideInInspector]
		public bool	isPinned;

		[NonSerialized]
		internal MixtureNode parentSettingsNode;
		[NonSerialized]
		internal MixtureNode childSettingsNode;

		CustomSampler		_sampler = null;
		CustomSampler		sampler
		{
			get
			{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
				if (_sampler == null)
				{
					_sampler = CustomSampler.Create($"{name} - {GetHashCode()}" , true);
					recorder = _sampler.GetRecorder();
					recorder.enabled = true;
				}
#endif
				return _sampler;
			}
		}

		protected Recorder	recorder { get; private set; }

		internal virtual float processingTimeInMillis
		{
			get
			{
				// By default we display the GPU processing time
				if (recorder != null)
					return recorder.gpuElapsedNanoseconds / 1000000.0f;
				return 0;
			}
		}

		protected MixtureSettings	Get2DOnlyRTSettings(MixtureSettings defaultSettings)
		{
			var rtSettings = defaultSettings;

			rtSettings.editFlags &= ~EditFlags.Dimension;
			rtSettings.dimension = OutputDimension.Texture2D;

			return rtSettings;
		}

		protected MixtureSettings	Get3DOnlyRTSettings(MixtureSettings defaultSettings)
		{
			var rtSettings = defaultSettings;

			rtSettings.editFlags &= ~EditFlags.Dimension;
			rtSettings.dimension = OutputDimension.Texture3D;

			return rtSettings;
		}

		protected MixtureSettings	GetCubeOnlyRTSettings(MixtureSettings defaultSettings)
		{
			var rtSettings = defaultSettings;

			rtSettings.editFlags &= ~EditFlags.Dimension;
			rtSettings.dimension = OutputDimension.CubeMap;

			return rtSettings;
		}

        public override void OnNodeCreated()
		{
			base.OnNodeCreated();
			settings = defaultSettings;
			previewMode = defaultPreviewChannels;
			previewSRGB = defaultPreviewSRGB;

			// Patch up inheritance mode with default value in graph
			onEnabled += () => settings.SyncInheritanceMode(graph.defaultNodeInheritanceMode);
		}

        protected override void Enable()
        {
            base.Enable();
			onAfterEdgeConnected += UpdateSettings;
			onAfterEdgeDisconnected += UpdateSettings;
			onSettingsChanged += UpdateSettings;
			UpdateSettings();
			onEnabled?.Invoke();
        }

		protected override void Disable()
		{
			foreach (var matKp in temporaryMaterials)
				CoreUtils.Destroy(matKp.Value);
			base.Disable();
			onAfterEdgeConnected -= UpdateSettings;
			onAfterEdgeDisconnected -= UpdateSettings;
			onSettingsChanged -= UpdateSettings;
		}


		public override void InitializePorts()
		{
			UpdateSettings();
			base.InitializePorts();
		}

		bool IsNodeUsingSettings(BaseNode n)
		{
			bool settings = n is MixtureNode m && m.hasSettings;

			// There are some exception where node don't have settings but we still inherit from them
			settings |= n is TextureNode;
			settings |= n is SelfNode;

			return true;
		}

		void UpdateSettings() => UpdateSettings(null);
		void UpdateSettings(SerializableEdge edge)
		{
			// Update nodes used to infere settings values
			parentSettingsNode = GetInputNodes().FirstOrDefault(n => IsNodeUsingSettings(n)) as MixtureNode;
			childSettingsNode = GetOutputNodes().FirstOrDefault(n => IsNodeUsingSettings(n)) as MixtureNode;

			settings.ResolveAndUpdate(this);
		}

		protected bool UpdateTempRenderTexture(ref CustomRenderTexture target, bool hasMips = false, bool autoGenerateMips = false,
			CustomRenderTextureUpdateMode updateMode = CustomRenderTextureUpdateMode.OnDemand, bool depthBuffer = false,
			GraphicsFormat overrideGraphicsFormat = GraphicsFormat.None, bool hideAsset = true)
		{
			if (graph.mainOutputTexture == null)
				return false;

			bool changed = false;
			int outputWidth = settings.GetResolvedWidth(graph);
			int outputHeight = settings.GetResolvedHeight(graph);
			int outputDepth = settings.GetResolvedDepth(graph);
			var filterMode = settings.GetResolvedFilterMode(graph);
			var wrapMode = settings.GetResolvedWrapMode(graph);
			GraphicsFormat targetFormat = overrideGraphicsFormat != GraphicsFormat.None ? overrideGraphicsFormat : settings.GetGraphicsFormat(graph);
			TextureDimension dimension = GetTempTextureDimension();

			outputWidth = Mathf.Max(outputWidth, 1);
			outputHeight = Mathf.Max(outputHeight, 1);
			outputDepth = Mathf.Max(outputDepth, 1);

			if (dimension != TextureDimension.Tex3D)
				outputDepth = 1;
			
			if (dimension == TextureDimension.Cube)
				outputHeight = outputDepth = outputWidth; // we only use the width for cubemaps

            if (targetFormat == GraphicsFormat.None)
                targetFormat = graph.mainOutputTexture.graphicsFormat;
			if (dimension == TextureDimension.None)
				dimension = TextureDimension.Tex2D;

			if (dimension == TextureDimension.Tex3D)
				depthBuffer = false;

			if (target == null)
			{
                target = new CustomRenderTexture(outputWidth, outputHeight, targetFormat)
                {
                    volumeDepth = Math.Max(1, outputDepth),
					depth = depthBuffer ? 32 : 0,
                    dimension = dimension,
                    name = $"Mixture Temp {name}",
                    updateMode = CustomRenderTextureUpdateMode.OnDemand,
                    doubleBuffered = settings.doubleBuffered,
                    wrapMode = settings.GetResolvedWrapMode(graph),
                    filterMode = settings.GetResolvedFilterMode(graph),
                    useMipMap = hasMips,
					autoGenerateMips = autoGenerateMips,
					enableRandomWrite = true,
					hideFlags = hideAsset ? HideFlags.HideAndDontSave : HideFlags.None,
					updatePeriod = settings.GetUpdatePeriodInMilliseconds(),
				};
				target.Create();
				target.material = MixtureUtils.dummyCustomRenderTextureMaterial;

				return true;
			}

			// TODO: check if format is supported by current system

			// Warning: here we use directly the settings from the 
			if (target.width != outputWidth
				|| target.height != outputHeight
				|| target.graphicsFormat != targetFormat
				|| target.dimension != dimension
				|| target.volumeDepth != outputDepth
				|| target.doubleBuffered != settings.doubleBuffered
				|| target.useMipMap != hasMips
				|| target.autoGenerateMips != autoGenerateMips)
			{
				target.Release();
				target.width = outputWidth;
				target.height = outputHeight;
				target.graphicsFormat = (GraphicsFormat)targetFormat;
				target.dimension = dimension;
				target.volumeDepth = outputDepth;
				target.depth = depthBuffer ? 32 : 0;
				target.doubleBuffered = settings.doubleBuffered;
                target.useMipMap = hasMips;
				target.autoGenerateMips = autoGenerateMips;
				target.enableRandomWrite = true;
				target.hideFlags = hideAsset ? HideFlags.HideAndDontSave : HideFlags.None;
				target.Create();
				if (target.material == null)
					target.material = MixtureUtils.dummyCustomRenderTextureMaterial;
				changed = true;
			}

			// Patch settings that don't require to re-create the texture
			target.updateMode = updateMode;
			target.updatePeriod = settings.GetUpdatePeriodInMilliseconds();
			target.wrapMode = settings.GetResolvedWrapMode(graph);
			target.filterMode = settings.GetResolvedFilterMode(graph);

			if (target.doubleBuffered)
			{
				target.EnsureDoubleBufferConsistency();
				var rt = target.GetDoubleBufferRenderTexture();
				if (rt.enableRandomWrite != true)
				{
					rt.Release();
					rt.enableRandomWrite = true;
					rt.Create();
				}
			}

			if (!target.IsCreated())
				target.Create();

			return changed;
		}

		protected virtual TextureDimension GetTempTextureDimension() => settings.GetResolvedTextureDimension(graph);

		public void OnProcess(CommandBuffer cmd)
		{
			inputPorts.PullDatas();

			UpdateSettings();

			ExceptionToLog.Call(() => Process(cmd));

			InvokeOnProcessed();

			outputPorts.PushDatas();
		}

		protected sealed override void Process() => throw new Exception("Do not use");

		void Process(CommandBuffer cmd)
		{
			var outputDimension = settings.GetResolvedTextureDimension(graph);

			if (!supportedDimensions.Contains((OutputDimension)outputDimension))
			{
				// AddMessage($"Dimension {outputDimension} is not supported by this node", NodeMessageType.Error);
				return ;
			}
			else
			{
				// TODO: simplify this with the node graph processor remove badges with matching words feature
				// RemoveMessage($"Dimension {TextureDimension.Tex2D} is not supported by this node");
				// RemoveMessage($"Dimension {TextureDimension.Tex3D} is not supported by this node");
				// RemoveMessage($"Dimension {TextureDimension.Cube} is not supported by this node");
			}

			beforeProcessSetup?.Invoke();

			// Avoid adding markers if it's CRT processing (CRT  already have one)
			// Or loops as it will bloat the debug markers
			bool loopNode = this is ILoopStart || this is ILoopEnd;
			if ((this is IUseCustomRenderTextureProcessing crt && crt.GetCustomRenderTextures().Count() > 0) || loopNode)
				ProcessNode(cmd);
			else
			{
				if (sampler != null) // samplers are null in non-dev builds
					cmd.BeginSample(sampler);
				ProcessNode(cmd);
				if (sampler != null) // samplers are null in non-dev builds
					cmd.EndSample(sampler);
			}
			afterProcessCleanup?.Invoke();
		}

		protected virtual bool ProcessNode(CommandBuffer cmd) => true;

		protected void RemoveObjectFromGraph(Object obj) => graph.RemoveObjectFromGraph(obj);

		protected Type GetPropertyType(Shader shader, int shaderPropertyIndex)
		{
			var type = shader.GetPropertyType(shaderPropertyIndex);

			switch (type)
			{
				case ShaderPropertyType.Color:
					return typeof(Color);
				case ShaderPropertyType.Float:
				case ShaderPropertyType.Range:
					return typeof(float);
				case ShaderPropertyType.Texture:
					return TextureUtils.GetTypeFromDimension(shader.GetPropertyTextureDimension(shaderPropertyIndex));
				default:
				case ShaderPropertyType.Vector:
					return typeof(Vector4);
			}
		}

		static Regex tooltipRegex = new Regex(@"Tooltip\((.*)\)");
		protected IEnumerable< PortData > GetMaterialPortDatas(Material material)
		{
			if (material == null)
				yield break;

			var currentDimension = settings.GetResolvedTextureDimension(graph);

			var s = material.shader;
			for (int i = 0; i < material.shader.GetPropertyCount(); i++)
			{
				var flags = s.GetPropertyFlags(i);
				var name = s.GetPropertyName(i);
				var displayName = s.GetPropertyDescription(i);
				var type = s.GetPropertyType(i);
				var tooltip = s.GetPropertyAttributes(i).Where(s => s.Contains("Tooltip")).FirstOrDefault();

				// Inspector only properties aren't available as ports.
				if (displayName.ToLower().Contains("[inspector]"))
					continue;

				if (tooltip != null)
				{
					// Parse tooltip:
					var m = tooltipRegex.Match(tooltip);
					tooltip = m.Groups[1].Value;
				}

				if ((flags & ShaderPropertyFlags.HideInInspector) != 0
					|| (flags & ShaderPropertyFlags.NonModifiableTextureData) != 0
					|| (flags & ShaderPropertyFlags.PerRendererData) != 0)
					continue;
				
				if (!PropertySupportsDimension(s.GetPropertyName(i), currentDimension))
					continue;

				// We don't display textures specific to certain dimensions if the node isn't in this dimension.
				if (type == ShaderPropertyType.Texture)
				{
					bool is2D = displayName.EndsWith(MixtureUtils.texture2DPrefix);
					bool is3D = displayName.EndsWith(MixtureUtils.texture3DPrefix);
					bool isCube = displayName.EndsWith(MixtureUtils.textureCubePrefix);

					if (is2D || is3D || isCube)
					{
						if (currentDimension == TextureDimension.Tex2D && !is2D)
							continue;
						if (currentDimension == TextureDimension.Tex3D && !is3D)
							continue;
						if (currentDimension == TextureDimension.Cube && !isCube)
							continue;
						displayName = Regex.Replace(displayName, @"_2D|_3D|_Cube", "", RegexOptions.IgnoreCase);
					}
				}

				yield return new PortData{
					identifier = name,
					displayName = displayName,
					displayType = GetPropertyType(s, i),
					tooltip = tooltip,
				};
			}
		}

		bool PropertySupportsDimension(string name, TextureDimension dim)
		{
			return MixtureUtils.GetAllowedDimentions(name).Contains(dim);
		}

		protected void AssignMaterialPropertiesFromEdges(List< SerializableEdge > edges, Material material)
		{
			// Update material settings when processing the graph:
			foreach (var edge in edges)
			{
				// Just in case something bad happened in a node
				if (edge.passThroughBuffer == null)
					continue;

				string propName = edge.inputPort.portData.identifier;
				int propertyIndex = material.shader.FindPropertyIndex(propName);

				if (propertyIndex == -1)
					continue;

				switch (material.shader.GetPropertyType(propertyIndex))
				{
					case ShaderPropertyType.Color:
						material.SetColor(propName, MixtureConversions.ConvertObjectToColor(edge.passThroughBuffer));
						break;
					case ShaderPropertyType.Texture:
						// TODO: texture scale and offset
						// Check texture dim before assigning:
						if (edge.passThroughBuffer is Texture t)
						{
							if (t == null || t.dimension == material.shader.GetPropertyTextureDimension(propertyIndex))
								material.SetTexture(propName, t);
						}
            else if (edge.passThroughBuffer == null)
            {
              material.SetTexture(propName, null);
            }
						break;
					case ShaderPropertyType.Float:
					case ShaderPropertyType.Range:
						switch (edge.passThroughBuffer)
						{
							case float f:
								material.SetFloat(propName, f);
								break;
							case Vector2 v:
								material.SetFloat(propName, v.x);
								break;
							case Vector3 v:
								material.SetFloat(propName, v.x);
								break;
							case Vector4 v:
								material.SetFloat(propName, v.x);
								break;
							case int i:
								material.SetFloat(propName, i);
								break;
							default:
								throw new Exception($"Can't assign {edge.passThroughBuffer.GetType()} to material float property");
						}
						break;
					case ShaderPropertyType.Vector:
						material.SetVector(propName, MixtureConversions.ConvertObjectToVector4(edge.passThroughBuffer));
						break;
				}
			}
		}

#if UNITY_EDITOR
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
#endif

		public void OnSettingsChanged()
		{
			onSettingsChanged?.Invoke();
			graph.NotifyNodeChanged(this);
		}

		Dictionary<Material, Material>		defaultMaterials = new Dictionary<Material, Material>();

		public Material	GetDefaultMaterial(Material mat)
		{
			Material defaultMat;

			if (defaultMaterials.TryGetValue(mat, out defaultMat))
				return defaultMat;
			
			return defaultMaterials[mat] = CoreUtils.CreateEngineMaterial(mat.shader);
		}

		public void ResetMaterialPropertyToDefault(Material mat, string propName)
		{
			if (mat == null)
				return;

			int idx = mat.shader.FindPropertyIndex(propName);
			if (idx == -1)
				return;

			switch (mat.shader.GetPropertyType(idx))
			{
				case ShaderPropertyType.Float:
				case ShaderPropertyType.Range:
					mat.SetFloat(propName, GetDefaultMaterial(mat).GetFloat(propName));
					break;
				case ShaderPropertyType.Vector:
					mat.SetVector(propName, GetDefaultMaterial(mat).GetVector(propName));
					break;
				case ShaderPropertyType.Texture:
					mat.SetTexture(propName, GetDefaultMaterial(mat).GetTexture(propName));
					break;
			}
		}

		public Material GetTempMaterial(string shaderName)
		{
			temporaryMaterials.TryGetValue(shaderName, out var material);

			if (material == null)
			{
				var shader = Shader.Find(shaderName);
				if (shader == null)
					throw new Exception("Can't find shader " + shaderName);
				material = temporaryMaterials[shaderName] = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };
			}

			return material;
		}

		// For all nodes that inherit MixtureNode, we provide a function that automatically changes the port type depending on the texture dimension:
		[CustomPortTypeBehavior(typeof(Texture))]
		[CustomPortTypeBehavior(typeof(RenderTexture))]
		[CustomPortTypeBehavior(typeof(CustomRenderTexture))]
		IEnumerable< PortData > GetTypeFromTextureDim(string fieldName, string displayName, object fieldValue)
		{
			bool input = false;

			var field = GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
			if (field.GetCustomAttribute<InputAttribute>() != null)
				input = true;

			yield return new PortData
			{
				displayName = displayName,
				displayType = TextureUtils.GetTypeFromDimension(settings.GetResolvedTextureDimension(graph)),
				identifier = fieldName,
				acceptMultipleEdges = input ? false : true,
			};
		}

		// Workaround to be able to have the same node with different port settings per graph texture dimension
		[IsCompatibleWithGraph]
		internal static bool IsNodeCompatibleWithGraph(BaseGraph graph) => true;
	}

	[Flags]
	public enum EditFlags
	{
		None			= 0,
		Width			= 1 << 0,
		SizeMode		= 1 << 1,
		Height			= 1 << 2,
		Depth			= 1 << 4,
		Dimension		= 1 << 6,
		TargetFormat	= 1 << 7,
		POTSize			= 1 << 8,

		Size			= SizeMode | Width | Height | Depth,
		Format			= POTSize | Dimension | TargetFormat,
		
		All				= ~0,
	}

	public enum POTSize
	{
		_32			= 32,
		_64			= 64,
		_128		= 128,
		_256		= 256,
		_512		= 512,
		_1024		= 1024,
		_2048		= 2048,
		_4096		= 4096,
		_8192		= 8192,
		Custom		= -1,
	}

	public enum OutputSizeMode
	{
		InheritFromGraph = NodeInheritanceMode.InheritFromGraph,
		InheritFromParent = NodeInheritanceMode.InheritFromParent,
		InheritFromChild = NodeInheritanceMode.InheritFromChild,
		Absolute = 1,
	}

	public enum OutputDimension
	{
		InheritFromGraph = NodeInheritanceMode.InheritFromGraph,
		InheritFromParent = NodeInheritanceMode.InheritFromParent,
		InheritFromChild = NodeInheritanceMode.InheritFromChild,
		Texture2D = TextureDimension.Tex2D,
		CubeMap = TextureDimension.Cube,
		Texture3D = TextureDimension.Tex3D,
		// Texture2DArray = TextureDimension.Tex2DArray, // Not supported by CRT, will be handled as Texture3D and then saved as Tex2DArray
	}

	public enum OutputPrecision
	{
		InheritFromGraph = NodeInheritanceMode.InheritFromGraph,
		InheritFromParent = NodeInheritanceMode.InheritFromParent,
		InheritFromChild = NodeInheritanceMode.InheritFromChild,
		LDR				= 2,
		Half			= 3,
		Full			= 4,
	}

	public enum OutputChannel
	{
		InheritFromGraph = NodeInheritanceMode.InheritFromGraph,
		InheritFromParent = NodeInheritanceMode.InheritFromParent,
		InheritFromChild = NodeInheritanceMode.InheritFromChild,
		RGBA = 1,
		RG = 2,
		R = 3,
	}

	public enum OutputWrapMode
	{
		InheritFromGraph = NodeInheritanceMode.InheritFromGraph,
		InheritFromParent = NodeInheritanceMode.InheritFromParent,
		InheritFromChild = NodeInheritanceMode.InheritFromChild,
		Repeat = TextureWrapMode.Repeat,
		Clamp = TextureWrapMode.Clamp,
		Mirror = TextureWrapMode.Mirror,
		MirrorOnce = TextureWrapMode.MirrorOnce,
	}

	public enum OutputFilterMode
	{
		InheritFromGraph = NodeInheritanceMode.InheritFromGraph,
		InheritFromParent = NodeInheritanceMode.InheritFromParent,
		InheritFromChild = NodeInheritanceMode.InheritFromChild,
		Point = FilterMode.Point,
		Bilinear = FilterMode.Bilinear,
		Trilinear = FilterMode.Trilinear,
	}

	[Flags]
    public enum PreviewChannels
    {
        R = 1,
        G = 2,
        B = 4,
        A = 8,
        RG = R | G,
        RB = R | B,
        GB = G | B,
        RGB = R | G | B,
        RGBA = R | G | B | A,
    }

	// Note: to keep in sync with UnityEditor.TextureCompressionQuality
	public enum MixtureCompressionQuality
	{
    	Fast     = 0,
    	Normal   = 50,
    	Best     = 100,
	}

	public enum RefreshMode
	{
		OnLoad,
		EveryXFrame,
		EveryXMillis,
		EveryXSeconds,
	}

	public static class MixtureEnumExtension 
	{
		public static bool Inherits(this OutputSizeMode mode) => (int)mode <= 0;
		public static bool Inherits(this OutputChannel mode) => (int)mode <= 0;
		public static bool Inherits(this OutputPrecision mode) => (int)mode <= 0;
		public static bool Inherits(this OutputDimension mode) => (int)mode <= 0;
		public static bool Inherits(this OutputWrapMode mode) => (int)mode < 0;
		public static bool Inherits(this OutputFilterMode mode) => (int)mode < 0;
	}
}