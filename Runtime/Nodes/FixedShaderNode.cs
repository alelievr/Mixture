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

		public virtual  float   width { get { return 340.0f; } }
		public abstract string  shaderName { get; }
		public abstract bool    displayMaterialInspector { get; }
		public virtual Precision precision { get { return Precision.SameAsOutput; } }

		public enum Precision
		{
			SameAsOutput,
			Byte,
			Float
		}

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
			return GetMaterialPortDatas(material);
		}

		[CustomPortInput(nameof(materialInputs), typeof(object))]
		public void GetMaterialInputs(List< SerializableEdge > edges)
		{
			AssignMaterialPropertiesFromEdges(edges, material);

		}

		protected override void Process()
		{
			GraphicsFormat graphicsFormat = GraphicsFormat.None;

			switch(precision)
			{
				case Precision.Byte: graphicsFormat = GraphicsFormat.R8G8B8A8_SRGB; break;
				case Precision.Float: graphicsFormat = GraphicsFormat.R32G32B32A32_SFloat; break;
			}

			UpdateTempRenderTexture(ref output, graphicsFormat);

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