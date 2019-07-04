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
	[System.Serializable]
	public abstract class FixedShaderNode : MixtureNode
	{
		[Input(name = "In"), SerializeField]
		public List<object>	materialInputs;

		[Output(name = "Out"), SerializeField]
		public RenderTexture	output = null;

		public Shader			shader;
		public override string	name => shader.name.Split('/').Last();
		public Material			material;

		public virtual  float   width => 340.0f;
		public abstract string  shaderName { get; }
		public abstract bool    displayMaterialInspector { get; }

		public override Texture previewTexture => output;

		public override MixtureRTSettings defaultRTSettings
		{
			get 
			{
                return new MixtureRTSettings()
                {
                    editFlags = EditFlags.All
            	};
			}
		}

        protected virtual IEnumerable<string> filteredOutProperties { get { return Enumerable.Empty<string>(); } }

		protected override void Enable()
		{
			if (shader == null)
			{
				shader = Shader.Find(shaderName);
			}

			if (material == null)
				material = new Material(shader);
		}

		[CustomPortBehavior(nameof(materialInputs))]
		protected virtual IEnumerable<PortData> ListMaterialProperties(List< SerializableEdge > edges)
		{
			foreach(var prop in base.GetMaterialPortDatas(material))
			{
				if(!filteredOutProperties.Contains(prop.identifier))
					yield return prop;
			}
		}

		[CustomPortInput(nameof(materialInputs), typeof(object))]
		public void GetMaterialInputs(List< SerializableEdge > edges)
		{
			AssignMaterialPropertiesFromEdges(edges, material);
		}

		protected override void Process()
		{
			UpdateTempRenderTexture(ref output);

			if (material == null)
			{
				Debug.LogError($"Can't process {name}, missing material/shader");
				return ;
			}

			// TODO: make this work wit Texture2DArray and Texture3D
			Graphics.Blit(Texture2D.whiteTexture, output, material, 0);
		}
	}
}