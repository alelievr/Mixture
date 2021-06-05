using UnityEngine;
using UnityEngine.Rendering;

namespace Mixture
{
	[System.Serializable]
	public abstract class FixedShaderNode : ShaderNode
	{
		public abstract string  shaderName { get; }
		public abstract bool    displayMaterialInspector { get; }
		public override Texture previewTexture => output;

		public override void InitializePorts()
		{
			UpdateShaderAndMaterial();
			base.InitializePorts();
		}

        protected override void Enable()
        {
			UpdateShaderAndMaterial();
            base.Enable();
        }

		void UpdateShaderAndMaterial()
		{
			if (shader == null)
				shader = Shader.Find(shaderName);

			if (material != null && material.shader != shader)
				material.shader = shader;

			if (material == null)
			{
				material = new Material(shader);
				material.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
			}
		}

		public override bool canProcess
		{
			get
			{
				UpdateShaderAndMaterial();
				return base.canProcess;
			}
		}
	}
}