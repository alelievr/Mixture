using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using UnityEngine.Rendering;
using System;

namespace Mixture
{
	[Documentation(@"
This node is the base node of all shader operations, it allows you to create a node with a custom behavior by putting a shader in the Shader field.
Note that the shader must be compatible with Custom Render Textures, otherwise it won't work. If you have a doubt you can create a new shader by pressing the button ""New Shader"".

The node will automatically reflect the shader properties as inputs that you'll be able to connect to other nodes.
This can be especially useful to prototype a new node or just add something that wasn't in the node Library.

For more information, you can check the [Shader Nodes](../ShaderNodes.md) documentation page.
")]

	[System.Serializable, NodeMenuItem("Shader")]
	public class ShaderNode : MixtureNode, IUseCustomRenderTextureProcessing
	{
		public static readonly string	DefaultShaderResourcePath = "ShaderNodeDefault";
		public static readonly string	DefaultShaderName = "Shader Graphs/ShaderNodeDefault";

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
			defaultShader = Resources.Load<Shader>(DefaultShaderResourcePath);

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

        protected override void Disable()
		{
			base.Disable();
			CoreUtils.Destroy(output);
		}

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
			var dim = rtSettings.GetTextureDimension(graph);

			if (output != null)
			{
				UpdateTempRenderTexture(ref output);
				dim = output.dimension;
			}

			yield return new PortData{
				displayName = "output",
				displayType = TextureUtils.GetTypeFromDimension(dim),
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
			if (output == null)
				return false;

			var outputDimension = rtSettings.GetTextureDimension(graph);
			MixtureUtils.SetupDimensionKeyword(material, outputDimension);

			var s = material.shader;
			for (int i = 0; i < s.GetPropertyCount(); i++)
			{
				if (s.GetPropertyType(i) != ShaderPropertyType.Texture)
					continue;

				int id = s.GetPropertyNameId(i);
				if (material.GetTexture(id) != null)
					continue; // Avoid overriding existing textures

				var dim = s.GetPropertyTextureDimension(i);
				if (dim == TextureDimension.Tex2D)
					continue; // Texture2D don't need this feature

				// default texture names doesn't work with cubemap and 3D textures so we do it ourselves...
				switch (s.GetPropertyTextureDefaultName(i))
				{
					case "black":
						material.SetTexture(id, TextureUtils.GetBlackTexture(dim));
						break;
					case "white":
						material.SetTexture(id, TextureUtils.GetWhiteTexture(dim));
						break;
					// TODO: grey and bump
				}
			}

			output.material = material;

			return true;
		}

        public IEnumerable<CustomRenderTexture> GetCustomRenderTextures()
		{
			yield return output;
		}
    }
}