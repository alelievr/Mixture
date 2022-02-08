using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using UnityEngine.Rendering;

namespace Mixture
{
	[Documentation(@"
Transform a 3D texture into a volume using an iso surface algorithm (Marching cubes currently).
")]

	[System.Serializable, NodeMenuItem("Mesh/IsoSurface (Marching Cubes)")]
	public class IsoSurface : ComputeShaderNode
	{
		[Input("Volume")]
		public Texture input;

		[Output("Mesh")]
		public MixtureMesh output;

		public float threshold;

		public override string name => "Marching Cubes";

		protected override string computeShaderResourcePath => "Mixture/MarchingCubes";

		public override bool showDefaultInspector => true;

		// TODO: do not use GetAssetPreview, it's super slow and only do low res previews.
		public override Texture previewTexture
		{
			get
			{
#if UNITY_EDITOR
				return output?.mesh != null ? UnityEditor.AssetPreview.GetAssetPreview(output.mesh) ?? Texture2D.blackTexture : Texture2D.blackTexture;
#else
				return Texture2D.blackTexture;
#endif
			}
		}

		public override List<OutputDimension> supportedDimensions => new List<OutputDimension>() {
			OutputDimension.Texture2D,
			OutputDimension.Texture3D,
		};

        ComputeBuffer vertices;
        ComputeBuffer normals;
        ComputeBuffer triangles;
        ComputeBuffer counterReadback;

		int marchingCubes;

		protected override void Enable()
		{
			base.Enable();

            // TODO: auto size from input volume
            int size = settings.GetResolvedWidth(graph) * settings.GetResolvedHeight(graph) * settings.GetResolvedDepth(graph);
            vertices = new ComputeBuffer(size * 15, sizeof(float) * 3, ComputeBufferType.Counter);
            normals = new ComputeBuffer(size * 15, sizeof(float) * 3, ComputeBufferType.Default);
            triangles = new ComputeBuffer(size * 6, sizeof(uint), ComputeBufferType.Default);
            counterReadback = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);

			marchingCubes = computeShader.FindKernel("MarchingCubes");
		}

        protected override void Disable()
        {
			base.Disable();
            vertices.Release();
            normals.Release();
            triangles.Release();
            counterReadback.Release();
        }

		protected override bool ProcessNode(CommandBuffer cmd)
		{
			if (!base.ProcessNode(cmd) || input == null)
				return false;

#if UNITY_2021_2_OR_NEWER
			cmd.SetBufferCounterValue(vertices, 0);
#else
			cmd.SetComputeBufferCounterValue(vertices, 0);
#endif

			// TODO: non pot texture3Ds
			cmd.SetComputeVectorParam(computeShader, "_VolumeSize", new Vector4(input.width, input.height, TextureUtils.GetSliceCount(input)));
			cmd.SetComputeFloatParam(computeShader, "_VoxelResolution", 1);
			cmd.SetComputeFloatParam(computeShader, "_Threshold", threshold);

			cmd.SetComputeTextureParam(computeShader, marchingCubes, "_VolumeTexture", input);
			cmd.SetComputeBufferParam(computeShader, marchingCubes, "_Vertices", vertices);
			cmd.SetComputeBufferParam(computeShader, marchingCubes, "_Normals", normals);
			cmd.SetComputeBufferParam(computeShader, marchingCubes, "_Triangles", triangles);
			DispatchCompute(cmd, marchingCubes, input.width, input.height, TextureUtils.GetSliceCount(input));

            MixtureGraphProcessor.AddGPUAndCPUBarrier(cmd);

            ComputeBuffer.CopyCount(vertices, counterReadback, 0);
            int[] count = new int[1];
            counterReadback.GetData(count);
            int vertexCount = count[0] * 3;

            // Readback all buffers
            Vector3[] vBuffer = new Vector3[vertexCount];
            vertices.GetData(vBuffer, 0, 0, vertexCount);
            int[] iBuffer = new int[vertexCount];
            triangles.GetData(iBuffer, 0, 0, vertexCount);

            var mesh = new Mesh { indexFormat = IndexFormat.UInt32 };
            mesh.vertices = vBuffer;
            mesh.indexFormat = IndexFormat.UInt32;
            // mesh.triangles = iBuffer;
            mesh.SetIndices(iBuffer, MeshTopology.Triangles, 0);
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            mesh.UploadMeshData(false);

            output = new MixtureMesh{ mesh = mesh };

			return true;
		}

		[CustomPortBehavior(nameof(input))]
		protected IEnumerable< PortData > ChangeOutputPortType(List< SerializableEdge > edges)
		{
			yield return new PortData{
				displayName = "Volume",
				displayType = typeof(Texture3D),
				identifier = "Volume",
				acceptMultipleEdges = false,
			};
		}
	}
}