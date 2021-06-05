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
		public enum Mode
		{
			[InspectorName("Use RGBA Channels")]
			AllChannels,
			[InspectorName("Use R Channel Only")]
			RChannelOnly
		}

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

		[Tooltip("Select the output mode for the render texture. The R Channel Only mode uses 4 times less memory.")]
		public Mode mode;

		[VisibleIf(nameof(mode), Mode.AllChannels)]
		[Tooltip("Default color that will be used to initialize the channels when using the RGBA channels mode.")]
		public Color neutralColor = new Color(0, 0, 0, 0);

		public override string	name => "Separate";

        public override bool hasPreview => false;
		public override bool showDefaultInspector => true;

		Material outputRMat, outputGMat, outputBMat, outputAMat;

		protected override void Enable()
		{
			SyncSettings();

            base.Enable();

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
			
			SyncSettings();

			UpdateTempRenderTexture(ref outputR);
			UpdateTempRenderTexture(ref outputG);
			UpdateTempRenderTexture(ref outputB);
			UpdateTempRenderTexture(ref outputA);

			SetMaterialParams(outputRMat, 0);
			SetMaterialParams(outputGMat, 1);
			SetMaterialParams(outputBMat, 2);
			SetMaterialParams(outputAMat, 3);

			void SetMaterialParams(Material m, int component)
			{
				MixtureUtils.SetTextureWithDimension(m, "_Source", input);
				m.SetColor("_NeutralColor", neutralColor);
				m.SetFloat("_Mode", (int)mode);
				m.SetInt("_Component", component);
			}
			return true;
		}

		void SyncSettings()
		{
			settings.outputChannels = mode == Mode.RChannelOnly ? OutputChannel.R : OutputChannel.RGBA;
			settings.editFlags &= ~EditFlags.Format;
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