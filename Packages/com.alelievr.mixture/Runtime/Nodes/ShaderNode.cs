using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using UnityEngine.Rendering;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Shader")]
	public class ShaderNode : MixtureNode, IUseCustomRenderTextureProcessing
	{
		public static readonly string	DefaultShaderName = "ShaderNodeDefault";

		[Input(name = "In")]
		public List< object >		materialInputs;

		[Output(name = "Out")]
		public CustomRenderTexture	output = null;

		public Shader			shader;
		public override string	name => (shader != null) ? shader.name.Split('/')?.Last() : "Shader";
		public Material			material;

        protected virtual IEnumerable<string> filteredOutProperties => Enumerable.Empty<string>();
		public override Texture previewTexture => output;

		Shader					defaultShader;

		protected override void Enable()
		{
			defaultShader = Resources.Load<Shader>(DefaultShaderName);

			if (material == null)
			{
				material = new Material(shader ?? defaultShader);
				material.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
			}
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

		protected override bool ProcessNode(CommandBuffer cmd)
		{
			UpdateTempRenderTexture(ref output);

			if (material == null || material.shader == null)
			{
				Debug.LogError($"Can't process {name}, missing material/shader.");
				return false;
			}

			var outputDimension = rtSettings.GetTextureDimension(graph);
			MixtureUtils.SetupDimensionKeyword(material, outputDimension);

#if UNITY_EDITOR // IsShaderCompiled is editor only
			if (!IsShaderCompiled(material.shader))
			{
				output.material = null;
				Debug.LogError($"Can't process {name}, shader has errors.");
				LogShaderErrors(material.shader);
			}
			else
#endif
			{
				output.material = material;
			}

			return true;
		}

        public CustomRenderTexture GetCustomRenderTexture() => output;
    }
}