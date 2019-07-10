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
		public RenderTexture	output = null;

		public Shader			shader;
		public override string	name => "Shader";
		public Material			material;

		public static string	DefaultShaderName = "ShaderNodeDefault";

		public int				sliceIndexMaterialProperty = Shader.PropertyToID("_SliceIndex");

		public override Texture previewTexture { get { return output;}}

		protected override void Enable()
		{
			if (shader == null)
			{
				shader = Resources.Load<Shader>(DefaultShaderName);
			}

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

		[CustomPortBehavior(nameof(output))]
		IEnumerable< PortData > ChangeOutputPortType(List< SerializableEdge > edges)
		{
			yield return new PortData{
				displayName = "output",
				displayType = TextureUtils.GetTypeFromDimension((TextureDimension)graph.outputNode.rtSettings.dimension),
				identifier = "outout",
			};
		}

		protected override void Process()
		{
			UpdateTempRenderTexture(ref output);

			if (material == null)
			{
				Debug.LogError($"Can't process {name}, missing material/shader");
				return ;
			}

			switch (output.dimension)
			{
				case TextureDimension.Tex2D:
				case TextureDimension.Tex2DArray:
				case TextureDimension.Tex3D:
					for (int i = 0; i < output.volumeDepth; i++)
					{
						if (material.HasProperty(sliceIndexMaterialProperty))
							material.SetInt(sliceIndexMaterialProperty, i);
						Graphics.Blit(Texture2D.whiteTexture, output, material, 0);
					}
					break ;
				default:
					Debug.LogError("Shader Node output not supported");
					break;
			}
		}
	}
}