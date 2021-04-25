using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using UnityEngine.Experimental.Rendering;
using System;

namespace Mixture
{
    [Documentation(@"
Transform a Mesh into a distance field. The distance field can be either signed or unsigned depending on the mode.

Note that the unsigned distance field is faster to compute.
")]

	[System.Serializable, NodeMenuItem("Mesh/Mesh To Distance Field"), NodeMenuItem("Mesh/Mesh To Volume")]
	public class MeshToUDF : ComputeShaderNode
	{
        public enum Mode
        {
            Signed,
            Unsigned,
        }

		[Input("Input Mesh"), ShowAsDrawer]
		public MixtureMesh inputMesh;

        [Output("Volume")]
        public CustomRenderTexture outputVolume;

        [Tooltip("Unsigned distance fields are faster to compute")]
        public Mode mode = Mode.Signed;

		[Tooltip("Enable Conservative rasterization when rendering the mesh. It can help to keep small details in the mesh."), ShowInInspector]
        public bool conservativeRaster = false;

		public override string	name => "Mesh To Distance Field";
		protected override string computeShaderResourcePath => "Mixture/MeshToSDF";

		public override Texture previewTexture => outputVolume;
		public override bool showDefaultInspector => true;

        CustomRenderTexture rayMapBuffer;

        MaterialPropertyBlock props;
        int clearUnsignedKernel;
        int clearSignedKernel;
        int fillUVUnsignedKernel;
        int fillUVSignedKernel;
        int jumpFloodingUnsignedKernel;
        int jumpFloodingSignedKernel;
        int finalPassUnsignedKernel;
        int finalPassSignedKernel;

		protected override void Enable()
		{
            base.Enable();
            rtSettings.editFlags = EditFlags.Dimension | EditFlags.Size;
            rtSettings.sizeMode = OutputSizeMode.Default;
            rtSettings.outputChannels = OutputChannel.RGBA;
            rtSettings.outputPrecision = OutputPrecision.Half;
            rtSettings.filterMode = FilterMode.Point;
            rtSettings.dimension = OutputDimension.Texture3D;
            UpdateTempRenderTexture(ref outputVolume);
            UpdateTempRenderTexture(ref rayMapBuffer, overrideGraphicsFormat: GraphicsFormat.R32_UInt);
            props = new MaterialPropertyBlock();

            clearUnsignedKernel = computeShader.FindKernel("ClearUnsigned");
            clearSignedKernel = computeShader.FindKernel("ClearSigned");
            fillUVUnsignedKernel = computeShader.FindKernel("FillUVMapUnsigned");
            fillUVSignedKernel = computeShader.FindKernel("FillUVMapSigned");
            jumpFloodingUnsignedKernel = computeShader.FindKernel("JumpFloodingUnsigned");
            jumpFloodingSignedKernel = computeShader.FindKernel("JumpFloodingSigned");
            finalPassUnsignedKernel = computeShader.FindKernel("FinalPassUnsigned");
            finalPassSignedKernel = computeShader.FindKernel("FinalPassSigned");
		}

        protected override void Disable()
        {
            base.Disable();
            CoreUtils.Destroy(outputVolume);
            CoreUtils.Destroy(rayMapBuffer);
        }

		protected override bool ProcessNode(CommandBuffer cmd)
		{
			rtSettings.doubleBuffered = true;

            if (!base.ProcessNode(cmd) || inputMesh?.mesh == null)
                return false;

            UpdateTempRenderTexture(ref outputVolume);
            UpdateTempRenderTexture(ref rayMapBuffer, overrideGraphicsFormat: GraphicsFormat.R32_UInt);
			MixtureUtils.SetupComputeTextureDimension(cmd, computeShader, TextureDimension.Tex3D);

			// Clear 3D render texture
            int clearKernel = mode == Mode.Signed ? clearSignedKernel : clearUnsignedKernel;
			cmd.SetComputeTextureParam(computeShader, clearKernel, "_Output", outputVolume);
			cmd.SetComputeTextureParam(computeShader, clearKernel, "_RayMapsOutput", rayMapBuffer);
			DispatchCompute(cmd, clearKernel, outputVolume.width, outputVolume.height, outputVolume.volumeDepth);

            // Rasterize the mesh in the volume
			MixtureUtils.RasterizeMeshToTexture3D(cmd, inputMesh, outputVolume, conservativeRaster);

            // Generate a distance field with JFA
            JumpFlooding(cmd);

			return true;
		}

        void JumpFlooding(CommandBuffer cmd)
        {
            // TODO: add non-cube support for JFA
			cmd.SetComputeVectorParam(computeShader, "_Size", new Vector4(outputVolume.width, 1.0f / outputVolume.width));

			var rt = outputVolume.GetDoubleBufferRenderTexture();
            var rayMapBuffer2 = rayMapBuffer.GetDoubleBufferRenderTexture();

            int jumpFloodingKernel = jumpFloodingUnsignedKernel;
            int fillUVKernel = fillUVUnsignedKernel;
            int finalPassKernel = finalPassUnsignedKernel;

            if (mode == Mode.Signed)
            {
                jumpFloodingKernel = jumpFloodingSignedKernel;
                fillUVKernel = fillUVSignedKernel;
                finalPassKernel = finalPassSignedKernel;
            }
            
            // TODO: try to get rid of the copies again
            TextureUtils.CopyTexture(cmd, outputVolume, rt);

            // Jump flooding implementation based on https://www.comp.nus.edu.sg/~tants/jfa.html
            // ANd signed version based on 'Generating signed distance fields on the GPU with ray maps'
			cmd.SetComputeTextureParam(computeShader, fillUVKernel, "_Input", rt);
			cmd.SetComputeTextureParam(computeShader, fillUVKernel, "_Output", outputVolume);
            cmd.SetComputeTextureParam(computeShader, fillUVKernel, "_RayMapsOutput", rayMapBuffer2);
			DispatchCompute(cmd, fillUVKernel, outputVolume.width, outputVolume.height, outputVolume.volumeDepth);

			int maxLevels = (int)Mathf.Log(outputVolume.width, 2);
			for (int i = 0; i <= maxLevels; i++)
			{
				float offset = 1 << (maxLevels - i);
				cmd.SetComputeFloatParam(computeShader, "_Offset", offset);
				cmd.SetComputeTextureParam(computeShader, jumpFloodingKernel, "_Input", outputVolume);
				cmd.SetComputeTextureParam(computeShader, jumpFloodingKernel, "_Output", rt);
                cmd.SetComputeTextureParam(computeShader, jumpFloodingKernel, "_RayMapsInput", rayMapBuffer2);
                cmd.SetComputeTextureParam(computeShader, jumpFloodingKernel, "_RayMapsOutput", rayMapBuffer);
				DispatchCompute(cmd, jumpFloodingKernel, outputVolume.width, outputVolume.height, outputVolume.volumeDepth);
				TextureUtils.CopyTexture(cmd, rt, outputVolume);
                if (mode == Mode.Signed)
                    TextureUtils.CopyTexture(cmd, rayMapBuffer, rayMapBuffer2);
			}

            // TODO: additional pass to compute an approximate "signness" (see the ray maps paper)

			cmd.SetComputeTextureParam(computeShader, finalPassKernel, "_Input", rt);
			cmd.SetComputeTextureParam(computeShader, finalPassKernel, "_Output", outputVolume);
            cmd.SetComputeTextureParam(computeShader, finalPassKernel, "_RayMapsInput", rayMapBuffer2);
            cmd.SetComputeTextureParam(computeShader, finalPassKernel, "_RayMapsOutput", rayMapBuffer);
			DispatchCompute(cmd, finalPassKernel, outputVolume.width, outputVolume.height, outputVolume.volumeDepth);
        }
    }
}