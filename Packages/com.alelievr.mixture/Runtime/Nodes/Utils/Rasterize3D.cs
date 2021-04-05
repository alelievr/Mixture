using UnityEngine;
using GraphProcessor;
using UnityEngine.Rendering;

namespace Mixture
{
    [Documentation(@"
Transform a Mesh into a distance field. The distance field can be either signed or unsigned depending on the mode.

Note that the unsigned distance field is faster to compute.
")]

	[System.Serializable, NodeMenuItem("Mesh/Rasterize 3D Mesh")]
	public class Rasterize3D : ComputeShaderNode
	{
		[Input("Input Mesh"), ShowAsDrawer]
		public MixtureMesh inputMesh;

        [Output("Volume")]
        public CustomRenderTexture outputVolume;

		[ShowInInspector, Tooltip("Enable Conservative rasterization when rendering the mesh. It can help to keep small details in the mesh.")]
        public bool conservativeRaster = false;

		public override string	name => "Rasterize Mesh 3D";
		protected override string computeShaderResourcePath => "Mixture/MeshToSDF";

		public override Texture previewTexture => outputVolume;
		public override bool showDefaultInspector => true;

        MaterialPropertyBlock props;

		protected override void Enable()
		{
            base.Enable();
            rtSettings.editFlags = 0;
            rtSettings.sizeMode = OutputSizeMode.Default;
            rtSettings.filterMode = FilterMode.Point;
            rtSettings.dimension = OutputDimension.Texture3D;
            UpdateTempRenderTexture(ref outputVolume);
            props = new MaterialPropertyBlock();
		}

        protected override void Disable()
        {
            base.Disable();
            CoreUtils.Destroy(outputVolume);
        }

		protected override bool ProcessNode(CommandBuffer cmd)
		{
            if (!base.ProcessNode(cmd) || inputMesh?.mesh == null)
                return false;

            UpdateTempRenderTexture(ref outputVolume);

			// Clear 3D render texture
			cmd.SetComputeTextureParam(computeShader, 0, "_Output", outputVolume);
			DispatchCompute(cmd, 0, outputVolume.width, outputVolume.height, outputVolume.volumeDepth);

			MixtureUtils.RasterizeMeshToTexture3D(cmd, inputMesh, outputVolume, conservativeRaster);

			return true;
		}
    }
}