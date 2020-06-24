using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using UnityEngine.Rendering;
using System;
using Net3dBool;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Mesh Operator")]
	public class MeshOperator : MixtureNode
	{
        public enum Operator
        {
            Union,
            Intersection,
            Difference,
        }

		[Input("Input Meshes", allowMultiple: true)]
		public List<MixtureMesh> inputMeshes = new List<MixtureMesh>();

        [Output("Result")]
        public MixtureMesh result;

        public Operator op;

		public override string	name => "Mesh Operator";

		public override Texture previewTexture => result?.mesh != null && !MixtureGraphProcessor.isProcessing ? UnityEditor.AssetPreview.GetAssetPreview(result.mesh) ?? (Texture)preview : (Texture)preview;
		CustomRenderTexture		preview;
		public override bool showDefaultInspector => true;

		protected override void Enable()
		{
			UpdateTempRenderTexture(ref preview);

			// Update temp RT after process in case RTSettings have been modified in Process()
			afterProcessCleanup += () => {
				UpdateTempRenderTexture(ref preview);
			};
		}

        protected override void Disable() => CoreUtils.Destroy(preview);

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
            if (inputMeshes.Count == 0)
                return false ;

            // Transform input meshes into solids:
            var solids = inputMeshes
                .Where(i => i.mesh != null && i.mesh.vertices.Length > 0)
                .Select(i => {
                    var s = new Solid(i.mesh.vertices, i.mesh.triangles, i.mesh.normals);
                    s.ApplyMatrix(i.localToWorld);
                    return s;
                })
                .ToList();

            Solid resultMesh = solids[0];
            if (inputMeshes.Count > 1)
            {
                for (int i = 1; i < solids.Count; i++)
                {
                    resultMesh = ApplyOperator(resultMesh, solids[i]);
                }
            }

            // Rebuild the baked mixture mesh:
            var finalMesh = new Mesh{ indexFormat = IndexFormat.UInt32 };
            finalMesh.vertices = resultMesh.getVertices().Select(v => new Vector3((float)v.x, (float)v.y, (float)v.z)).ToArray();
            finalMesh.triangles = resultMesh.getIndices();
            finalMesh.normals = resultMesh.getNormals();
            result = new MixtureMesh{ mesh = finalMesh, localToWorld = Matrix4x4.identity };

			return true;
		}

        Solid ApplyOperator(Solid a, Solid b)
        {
            BooleanModeller modeller = new BooleanModeller(a, b);
            Solid result;

            switch (op)
            {
                case Operator.Difference:
                    result = modeller.getDifference();
                    break;
                case Operator.Intersection:
                    result = modeller.getIntersection();
                    break;
                default:
                case Operator.Union:
                    result = modeller.getUnion();
                    break;
            }

            return result;
        }
    }
}