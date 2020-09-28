using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;
using System;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Mesh/Rasterize 3D Mesh"), NodeMenuItem("Mesh/Mesh To Volume")]
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
        public CustomRenderTexture outputVolume;

        public float renderingVolumeSize = 1;

		public override string	name => "Mesh To UDF";
		protected override string computeShaderResourcePath => "Mixture/MeshToSDF";

        public Resolution resolution = Resolution._128;

		public override Texture previewTexture => outputVolume;
		public override bool showDefaultInspector => true;

        MaterialPropertyBlock props;

		protected override void Enable()
		{
            base.Enable();
            rtSettings.editFlags = 0;
            rtSettings.sizeMode = OutputSizeMode.Fixed;
            rtSettings.width = rtSettings.height = rtSettings.sliceCount = (int)resolution;
            rtSettings.outputChannels = OutputChannel.RGBA;
            rtSettings.outputPrecision = OutputPrecision.Full;
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

		[CustomPortBehavior(nameof(inputMeshes))]
		public IEnumerable< PortData > InputMeshesDisplayType(List< SerializableEdge > edges)
		{
            yield return new PortData
            {
                identifier = nameof(inputMeshes),
                displayName = "Input Meshes",
                acceptMultipleEdges = true,
                displayType = typeof(MixtureMesh),
            };
		}

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

        [CustomPortBehavior(nameof(outputVolume))]
		public IEnumerable< PortData > ListMaterialProperties(List< SerializableEdge > edges)
        {
            yield return new PortData
            {
                identifier = nameof(outputVolume),
                displayName = "Volume",
                displayType = typeof(Texture3D),
                acceptMultipleEdges = true,
            };
        }

		protected override bool ProcessNode(CommandBuffer cmd)
		{
            if (!base.ProcessNode(cmd))
                return false;

            // Patch rtsettings with correct resolution input
            rtSettings.width = rtSettings.height = rtSettings.sliceCount = (int)resolution;
            UpdateTempRenderTexture(ref outputVolume);

            // Render the input meshes into the 3D volume:
            foreach (var mesh in inputMeshes)
            {
                if (mesh?.mesh == null)
                    continue;

                cmd.SetComputeTextureParam(computeShader, 0, "_Output", outputVolume);
                DispatchCompute(cmd, 0, outputVolume.width, outputVolume.height, outputVolume.volumeDepth);

                var mat = GetTempMaterial(rasterize3DShader);
                mat.SetVector("_OutputSize", new Vector4(outputVolume.width, 1.0f / (float)outputVolume.width));
                cmd.SetRandomWriteTarget(2, outputVolume);
                cmd.GetTemporaryRT(42, (int)outputVolume.width, (int)outputVolume.height, 0);
                cmd.SetRenderTarget(42);
                RenderMesh(Quaternion.Euler(90, 0, 0));
                RenderMesh(Quaternion.Euler(0, 90, 0));
                RenderMesh(Quaternion.Euler(0, 0, 90));
                cmd.ClearRandomWriteTargets();

                void RenderMesh(Quaternion cameraRotation)
                {
                    var worldToCamera = Matrix4x4.Rotate(cameraRotation);
                    var projection = Matrix4x4.Ortho(-renderingVolumeSize, renderingVolumeSize, -renderingVolumeSize, renderingVolumeSize, -renderingVolumeSize, renderingVolumeSize); // Rendering bounds
                    var vp = projection * worldToCamera;
                    props.SetMatrix("_CameraMatrix", vp);
                    cmd.DrawMesh(mesh.mesh, mesh.localToWorld, mat, 0, shaderPass: 0, props);
                }
            }

			return true;
		}
    }
}