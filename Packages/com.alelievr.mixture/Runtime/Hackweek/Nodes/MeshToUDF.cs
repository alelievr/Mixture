using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;
using System;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Mesh To UDF"), NodeMenuItem("Mesh To Volume")]
	public class MeshToUDF : ComputeShaderNode
	{
        public enum Resolution
        {
            _16 = 16,
            _32 = 32,
            _64 = 64,
            _128 = 128,
            _256 = 256,
            _512 = 512,
            _1024 = 1024,
        }
        
        const string rasterize3DShader = "Hidden/Mixture/Rasterize3D";

		[Input("Input Meshes", allowMultiple: true)]
		public List<MixtureMesh> inputMeshes = new List<MixtureMesh>();

        [Output("Volume")]
        public CustomRenderTexture sdf;

		public override string	name => "Mesh To UDF";
		protected override string computeShaderResourcePath => "Mixture/MeshToSDF";

        public Resolution resolution = Resolution._128;

		public override Texture previewTexture => sdf;
		public override bool showDefaultInspector => true;

        MaterialPropertyBlock props;

		protected override void Enable()
		{
            base.Enable();
            rtSettings.editFlags = 0;
            rtSettings.sizeMode = OutputSizeMode.Fixed;
            rtSettings.width = rtSettings.height = rtSettings.sliceCount = (int)resolution;
            rtSettings.targetFormat = OutputFormat.RGBA_Float;
            rtSettings.filterMode = FilterMode.Point;
            rtSettings.dimension = OutputDimension.Texture3D;
            UpdateTempRenderTexture(ref sdf);
            props = new MaterialPropertyBlock();
		}

        protected override void Disable() => CoreUtils.Destroy(sdf);

		// Functions with Attributes must be either protected or public otherwise they can't be accessed by the reflection code
		// [CustomPortBehavior(nameof(inputMeshes))]
		// public IEnumerable< PortData > ListMaterialProperties(List< SerializableEdge > edges)
		// {
        //     yield return new PortData
        //     {
        //         identifier = nameof(inputMeshes),
        //         displayName = "Input Meshes",
        //         allowMultiple = true,
        //         displayType()
        //     };
		// }

		[CustomPortInput(nameof(inputMeshes), typeof(MixtureMesh))]
		protected void GetMaterialInputs(List< SerializableEdge > edges)
		{
            if (inputMeshes == null)
                inputMeshes = new List<MixtureMesh>();
            inputMeshes.Clear();
			foreach (var edge in edges)
            {
                if (edge.passThroughBuffer is MixtureMesh m)
                    inputMeshes.Add(m);
            }
		}

		protected override bool ProcessNode(CommandBuffer cmd)
		{
            if (!base.ProcessNode(cmd))
                return false;

            UpdateTempRenderTexture(ref sdf);

            // Render the input meshes into the 3D volume:
            foreach (var mesh in inputMeshes)
            {
                if (mesh?.mesh == null)
                    continue;

                cmd.SetComputeTextureParam(computeShader, 0, "_Output", sdf);
                DispatchCompute(cmd, 0, sdf.width, sdf.height, sdf.volumeDepth);

                var mat = GetTempMaterial(rasterize3DShader);
                // mat.SetTexture("_Output", sdf);
                mat.SetVector("_OutputSize", new Vector4(sdf.width, 1.0f / (float)sdf.width));
                cmd.SetRandomWriteTarget(2, sdf);
                cmd.GetTemporaryRT(42, (int)sdf.width, (int)sdf.height, 0);
                cmd.SetRenderTarget(42);
                props.SetFloat("_Dir", 0);
                cmd.DrawMesh(mesh.mesh, mesh.localToWorld, mat, 0, shaderPass: 0, props);
                props.SetFloat("_Dir", 1);
                cmd.DrawMesh(mesh.mesh, mesh.localToWorld, mat, 0, shaderPass: 0, props);
                props.SetFloat("_Dir", 2);
                cmd.DrawMesh(mesh.mesh, mesh.localToWorld, mat, 0, shaderPass: 0, props);
                cmd.ClearRandomWriteTargets();
            }

			return true;
		}
    }
}