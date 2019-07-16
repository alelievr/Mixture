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
	public abstract class FixedShaderNode : ShaderNode
	{
		public override string	name => shader.name.Split('/').Last();

		public virtual  float   width => 340.0f; // TODO: factorise this and remove nodeViewSize in the outputnodeView
		public abstract string  shaderName { get; }
		public abstract bool    displayMaterialInspector { get; }

		public override Texture previewTexture => output;

		public override MixtureRTSettings defaultRTSettings
		{
			get => new MixtureRTSettings()
			{
				editFlags = EditFlags.All
			};
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
	}
}