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

		internal override float processingTimeInMillis
		{
			get
			{
				var sampler = CustomTextureManager.GetCustomTextureProfilingSampler(output);
				if (sampler != null)
					return sampler.GetRecorder().gpuElapsedNanoseconds / 1000000.0f;
				return 0;
			}
		}

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

		protected override bool ProcessNode(CommandBuffer cmd)
		{
			if (!base.ProcessNode(cmd))
				return false;

			if (shader == null)
				FindShader();
			
			if (shader == null)
				return false;
			
			return true;
		}
	}
}