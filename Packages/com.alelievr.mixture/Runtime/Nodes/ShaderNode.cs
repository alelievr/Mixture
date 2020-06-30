using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using UnityEngine.Rendering;
using System;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Shader")]
	public class ShaderNode : MixtureNode, IUseCustomRenderTextureProcessing
	{
		public static readonly string	DefaultShaderName = "ShaderNodeDefault";

		[Input(name = "In")]
		public List< object >		materialInputs;

		[Output(name = "Out"), Tooltip("Output Texture")]
		public CustomRenderTexture	output = null;

		public Shader			shader;
		public override string	name => (shader != null) ? shader.name.Split('/')?.Last() : "Shader";
		public Material			material;

        protected virtual IEnumerable<string> filteredOutProperties => Enumerable.Empty<string>();
		public override Texture previewTexture => output;

		internal override float processingTimeInMillis
		{
			get
			{
				var sampler = CustomTextureManager.GetCustomTextureProfilingSampler(output);
				if (sampler != null)
					return sampler.GetRecorder().gpuElapsedNanoseconds / 1000000.0f;
				return 0;
			}
		}

		Shader					defaultShader;

		protected override void Enable()
		{
			defaultShader = Resources.Load<Shader>(DefaultShaderName);

			if (material == null)
			{
				material = new Material(shader ?? defaultShader);
				material.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
			}

			beforeProcessSetup += BeforeProcessSetup;

			UpdateShader();
			UpdateTempRenderTexture(ref output);
			output.material = material;

			// Update temp RT after process in case RTSettings have been modified in Process()
			afterProcessCleanup += () => {
				UpdateTempRenderTexture(ref output);
			};
		}

        protected override void Disable() => CoreUtils.Destroy(output);

		// Functions with Attributes must be either protected or public otherwise they can't be accessed by the reflection code
		[CustomPortBehavior(nameof(materialInputs))]
		public IEnumerable< PortData > ListMaterialProperties(List< SerializableEdge > edges)
		{
			foreach (var p in GetMaterialPortDatas(material))
			{
				if (filteredOutProperties.Contains(p.identifier))
					continue;
				yield return p;
			}
		}

		[CustomPortInput(nameof(materialInputs), typeof(object))]
		protected void GetMaterialInputs(List< SerializableEdge > edges)
		{
			AssignMaterialPropertiesFromEdges(edges, material);
		}

		[CustomPortBehavior(nameof(output))]
		protected IEnumerable< PortData > ChangeOutputPortType(List< SerializableEdge > edges)
		{
			yield return new PortData{
				displayName = "output",
				displayType = TextureUtils.GetTypeFromDimension(rtSettings.GetTextureDimension(graph)),
				identifier = "output",
				acceptMultipleEdges = true,
			};
		}

		void UpdateShader()
		{
#if UNITY_EDITOR
			if (shader != null && material.shader != shader)
				material.shader = shader;
#endif
		}

		void BeforeProcessSetup()
		{
			UpdateShader();
			UpdateTempRenderTexture(ref output);
		}

		public override bool canProcess => IsShaderValid();

		internal bool IsShaderValid()
		{
			ClearMessages();

			if (material == null || material.shader == null)
			{
				AddMessage("missing material/shader", NodeMessageType.Error);
				return false;
			}
#if UNITY_EDITOR // IsShaderCompiled is editor only
			if (!IsShaderCompiled(material.shader))
			{
				output.material = null;
				foreach (var m in UnityEditor.ShaderUtil.GetShaderMessages(material.shader).Where(m => m.severity == UnityEditor.Rendering.ShaderCompilerMessageSeverity.Error))
				{
					string file = String.IsNullOrEmpty(m.file) ? material.shader.name : m.file;
					AddMessage($"{file}:{m.line} {m.message}", NodeMessageType.Error);
				}
				return false;
			}
#endif
			return true;
		}

		protected override bool ProcessNode(CommandBuffer cmd)
		{
			var outputDimension = rtSettings.GetTextureDimension(graph);
			MixtureUtils.SetupDimensionKeyword(material, outputDimension);
			output.material = material;

			return true;
		}

        public CustomRenderTexture GetCustomRenderTexture() => output;
    }
}