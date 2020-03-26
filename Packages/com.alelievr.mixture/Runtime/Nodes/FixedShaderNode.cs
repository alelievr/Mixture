using UnityEngine;

namespace Mixture
{
	[System.Serializable]
	public abstract class FixedShaderNode : ShaderNode
	{
		public abstract string  shaderName { get; }
		public abstract bool    displayMaterialInspector { get; }
        public virtual bool hasPreview => true;
		public override Texture previewTexture => output;

		protected override MixtureRTSettings defaultRTSettings
		{
			get
			{
                var settings = base.defaultRTSettings;
                settings.editFlags = EditFlags.All ^ EditFlags.POTSize;
                return settings;
			}
		}

		protected override void Enable()
		{
			if (shader == null)
				FindShader();

			base.Enable();
        }

		void FindShader() => shader = Shader.Find(shaderName);

		protected override bool ProcessNode()
		{
			if (!base.ProcessNode())
				return false;

			if (shader == null)
				FindShader();
			
			if (shader == null)
				return false;
			
			return true;
		}
	}
}