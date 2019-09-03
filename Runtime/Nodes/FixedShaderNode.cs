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

		public abstract string  shaderName { get; }
		public abstract bool    displayMaterialInspector { get; }

		public override Texture previewTexture => output;

		protected override MixtureRTSettings defaultRTSettings
		{
			get
			{
                var settings = base.defaultRTSettings;
                settings.editFlags = EditFlags.All;
                return settings;
			}
		}

		protected override void Enable()
		{
			if (shader == null)
			{
				shader = Shader.Find(shaderName);
			}

			base.Enable();
        }
	}
}