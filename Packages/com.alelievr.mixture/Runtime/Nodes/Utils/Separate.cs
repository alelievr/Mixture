using UnityEngine;
using GraphProcessor;
using UnityEngine.Rendering;
using System.Collections.Generic;

namespace Mixture
{
	[Documentation(@"
Separates the 4 components (RGBA) of the input texture into 4 R channel texture.
")]

	[System.Serializable, NodeMenuItem("Operators/Separate"), NodeMenuItem("Operators/Split")]
	public class Separate : MixtureNode, IUseCustomRenderTextureProcessing
	{
        [Input]
        public Texture input;

        [Output("R")]
        public CustomRenderTexture outputR;
        [Output("G")]
        public CustomRenderTexture outputG;
        [Output("B")]
        public CustomRenderTexture outputB;
        [Output("A")]
        public CustomRenderTexture outputA;

		public override string	name => "Separate";

        public override bool hasPreview => false;

		Material outputRMat, outputGMat, outputBMat, outputAMat;

		protected override void Enable()
		{
            base.Enable();
			settings.outputChannels = OutputChannel.R;
			settings.editFlags &= ~EditFlags.Format;

			UpdateTempRenderTexture(ref outputR);
			UpdateTempRenderTexture(ref outputG);
			UpdateTempRenderTexture(ref outputB);
			UpdateTempRenderTexture(ref outputA);
			
			var mat = GetTempMaterial("Hidden/Mixture/Separate");
			outputRMat = new Material(mat){ hideFlags = HideFlags.HideAndDontSave };
			outputGMat = new Material(mat){ hideFlags = HideFlags.HideAndDontSave };
			outputBMat = new Material(mat){ hideFlags = HideFlags.HideAndDontSave };
			outputAMat = new Material(mat){ hideFlags = HideFlags.HideAndDontSave };

			outputR.material = outputRMat;
			outputG.material = outputGMat;
			outputB.material = outputBMat;
			outputA.material = outputAMat;
		}

		protected override bool ProcessNode(CommandBuffer cmd)
		{
            if (!base.ProcessNode(cmd) || input == null)
                return false;

			UpdateTempRenderTexture(ref outputR);
			UpdateTempRenderTexture(ref outputG);
			UpdateTempRenderTexture(ref outputB);
			UpdateTempRenderTexture(ref outputA);

			MixtureUtils.SetTextureWithDimension(outputRMat, "_Source", input);
			MixtureUtils.SetTextureWithDimension(outputGMat, "_Source", input);
			MixtureUtils.SetTextureWithDimension(outputBMat, "_Source", input);
			MixtureUtils.SetTextureWithDimension(outputAMat, "_Source", input);
			outputRMat.SetInt("_Component", 0);
			outputGMat.SetInt("_Component", 1);
			outputBMat.SetInt("_Component", 2);
			outputAMat.SetInt("_Component", 3);

			return true;
		}

        protected override void Disable()
		{
			base.Disable();
			CoreUtils.Destroy(outputRMat);
			CoreUtils.Destroy(outputGMat);
			CoreUtils.Destroy(outputBMat);
			CoreUtils.Destroy(outputAMat);
		}

        public IEnumerable<CustomRenderTexture> GetCustomRenderTextures()
        {
			yield return outputR;
			yield return outputG;
			yield return outputB;
			yield return outputA;
        }
    }
}