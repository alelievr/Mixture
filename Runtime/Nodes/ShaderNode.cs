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
		[Input(name = "In")]
		public List< object >		materialInputs;

		[Output(name = "Out")]
		public CustomRenderTexture	output = null;

		public Shader			shader;
		public override string	name => "Shader";
		public Material			material;


		public static string	DefaultShaderName = "ShaderNodeDefault";

		public int				sliceIndexMaterialProperty = Shader.PropertyToID("_SliceIndex");

        protected virtual IEnumerable<string> filteredOutProperties => Enumerable.Empty<string>();

		public override Texture previewTexture => output;

		protected override void Enable()
		{
			if (shader == null)
			{
				shader = Resources.Load<Shader>(DefaultShaderName);
			}

			if (material == null)
				material = new Material(shader);
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
				displayType = TextureUtils.GetTypeFromDimension((TextureDimension)rtSettings.dimension),
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