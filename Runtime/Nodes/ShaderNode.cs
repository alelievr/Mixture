using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;
using GraphProcessor;
using System.Linq;
using UnityEditor;
using System;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Shader")]
	public class ShaderNode : MixtureNode
	{
		public static readonly string	DefaultShaderName = "ShaderNodeDefault";

		[Input(name = "In")]
		public List< object >		materialInputs;

		[Output(name = "Out")]
		public CustomRenderTexture	output = null;

		public Shader			shader;
		public override string	name => "Shader";
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

		// Functions with Attributes must be either protected or public otherwise they can't be accessed by the reflection code
		[CustomPortBehavior(nameof(materialInputs))]
		protected IEnumerable< PortData > ListMaterialProperties(List< SerializableEdge > edges)
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
			};
		}

		protected override void Process()
		{
			UpdateTempRenderTexture(ref output);

			if (material == null || material.shader == null)
			{
				Debug.LogError($"Can't process {name}, missing material/shader.");
				return ;
			}

			MixtureUtils.SetupDimensionKeyword(material, rtSettings.GetTextureDimension(graph));

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
		}
	}
}