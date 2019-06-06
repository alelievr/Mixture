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
		[Input(name = "In"), SerializeField]
		public List< object >	materialInputs;

		[Output(name = "Out"), SerializeField]
		public RenderTexture	output;

		public Shader			shader;
		public override string	name => "Shader";
		public Material			material;

		public static string	DefaultShaderName = "ShaderNodeDefault";

		protected override void Enable()
		{
			if (shader == null)
			{
				shader = Resources.Load<Shader>(DefaultShaderName);
			}

			// For now, we allocate one render target per node
			output = new RenderTexture(512, 512, 0, GraphicsFormat.R8G8B8A8_UNorm);

			if (material == null)
				material = new Material(shader);
		}

		[CustomPortBehavior(nameof(materialInputs))]
		IEnumerable< PortData > ListMaterialProperties(List< SerializableEdge > edges)
		{
			return GetMaterialPortDatas(material);
		}

		[CustomPortInput(nameof(materialInputs), typeof(object))]
		public void GetMaterialInputs(List< SerializableEdge > edges)
		{
			AssignMaterialPropertiesFromEdges(edges, material);
		}

		protected override void Process()
		{
			if (material == null)
			{
				Debug.LogError($"Can't process {name}, missing material/shader");
				return ;
			}

			Graphics.Blit(Texture2D.whiteTexture, output, material, 0);
		}
	}
}