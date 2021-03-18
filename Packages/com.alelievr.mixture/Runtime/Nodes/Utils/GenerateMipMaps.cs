using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using UnityEngine.Rendering;
using System.Reflection;

namespace Mixture
{
	[Documentation(@"
Generate mipmaps for the input texture.
")]

	[System.Serializable, NodeMenuItem("Utils/Generate MipMaps")]
	public class GenerateMipMaps : ShaderNode
	{
		public enum Mode
		{
			Auto,
			Gaussian,
			Max,
			Custom,
		}

		[Input(name = "Input Texture")]
		public Texture	input;

		public Mode mode;

		public override string	name => "Generate MipMaps";
		public override Texture previewTexture => output;
		public override bool showDefaultInspector => true;

		protected override bool hasMips => true;

        protected override IEnumerable<string> filteredOutProperties
		{
			get 
			{
				if (mode != Mode.Custom)
				{
					int count = material.shader.GetPropertyCount();
					for (int i = 0; i < count; i++)
						yield return material.shader.GetPropertyName(i);
				}
				else
					yield break;
			}
		}

		public override IEnumerable<FieldInfo> OverrideFieldOrder(IEnumerable<FieldInfo> fields)
		{
			return fields.OrderBy(f1 => {
				if (f1.Name == nameof(input))
					return 1;
				else
					return 0;
			});
		}

        protected override void Enable()
        {
			rtSettings.doubleBuffered = true;
            base.Enable();
        }

		protected override bool ProcessNode(CommandBuffer cmd)
		{
			rtSettings.doubleBuffered = true;
			if (!base.ProcessNode(cmd) || input == null)
				return false;

			TextureUtils.CopyTexture(cmd, input, output, false);

			var mipmapGenMat = GetTempMaterial("Hidden/Mixture/GenerateMipMaps");
			if (mode == Mode.Custom)
				mipmapGenMat = material;
			else
				output.material = null;
			
			if (mode == Mode.Auto)
			{
				cmd.GenerateMips(output);
			}
			else
			{
				mipmapGenMat.SetFloat("_Mode", (int)mode);
				MixtureUtils.SetTextureWithDimension(mipmapGenMat, "_PreviousMip", input);
				var props = new MaterialPropertyBlock();
				// Manually generate mips:
				for (int i = 0; i < output.mipmapCount - 1; i++)
				{
					props.SetFloat("_SourceMip", i);
					float width = Mathf.Max(1, input.width >> i);
					float height = Mathf.Max(1, input.width >> i);
					float depth = Mathf.Max(1, TextureUtils.GetSliceCount(input) >> i);
					props.SetVector("_RcpTextureSize", new Vector4(1.0f / width, 1.0f / height, 1.0f / depth, 0.0f));
					output.material = mipmapGenMat;

					if (mode == Mode.Gaussian)
					{
						// 2 passes of gaussian blur for 2D and Cubemaps
						props.SetVector("_GaussianBlurDirection", Vector3.right);
						CustomTextureManager.UpdateCustomRenderTexture(cmd, output, 1, i + 1, props);
						cmd.CopyTexture(output.GetDoubleBufferRenderTexture(), 0, i + 1, output, 0, i + 1);

						props.SetFloat("_SourceMip", i + 1);
						MixtureUtils.SetTextureWithDimension(props, "_PreviousMip", output);
						props.SetVector("_GaussianBlurDirection", Vector3.up);
						CustomTextureManager.UpdateCustomRenderTexture(cmd, output, 1, i + 1, props);

						// And a third pass if we're in 3D
						if (input.dimension == TextureDimension.Tex3D)
						{
							props.SetVector("_GaussianBlurDirection", Vector3.forward);
							cmd.CopyTexture(output.GetDoubleBufferRenderTexture(), 0, i + 1, output, 0, i + 1);
							CustomTextureManager.UpdateCustomRenderTexture(cmd, output, 1, i + 1, props);
						}
					}
					else
					{
						CustomTextureManager.UpdateCustomRenderTexture(cmd, output, 1, i + 1, props);
					}

					// TODO: handle cube & 3D textures
					// CoreUtils.SetRenderTarget(cmd, output, miplevel: i + 1);
					// CoreUtils.DrawFullScreen(cmd, mipmapGenMat, props);
					// cmd.DrawProcedural(Matrix4x4.identity, mipmapGenMat, 0, MeshTopology.Triangles, 6, 1, props);
					cmd.CopyTexture(output.GetDoubleBufferRenderTexture(), 0, i + 1, output, 0, i + 1);
					MixtureUtils.SetTextureWithDimension(props, "_PreviousMip", output);
				}
			}
			output.material = null;

			return true;
		}
    }
}