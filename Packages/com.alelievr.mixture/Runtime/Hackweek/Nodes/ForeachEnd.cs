using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using UnityEngine.Rendering;
using System;
using Net3dBool;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Foreach End")]
	public class ForeachEnd : MixtureNode
	{
		public enum AggregationMode
		{
			MergeResult,
			None,
		}

		[Input("Input")]
		public MixtureMesh inputMesh;

        [Output("Output")]
        public MixtureMesh output;

		public override string	name => "Foreach End";

		public override bool    hasPreview => false;
		public override bool    showDefaultInspector => true;

		public AggregationMode mode;

		protected override void Enable()
		{
		}

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

		// [CustomPortInput(nameof(inputMeshes), typeof(MixtureMesh))]
		// protected void GetMaterialInputs(List< SerializableEdge > edges)
		// {
        //     if (inputMeshes == null)
        //         inputMeshes = new List<MixtureMesh>();
        //     inputMeshes.Clear();
		// 	foreach (var edge in edges)
        //     {
        //         if (edge.passThroughBuffer is MixtureMesh m)
        //             inputMeshes.Add(m);
        //     }
		// }

		protected override bool ProcessNode(CommandBuffer cmd)
		{
			switch (mode)
			{
				case AggregationMode.MergeResult:
					CombineInputMeshes();
					break;
				default:
				case AggregationMode.None:
					output = inputMesh.Clone();
					break;
			}
			return true;
		}

		void CombineInputMeshes()
		{
			if (inputMesh == null || inputMesh.mesh == null)
				return;
			
			inputMeshes.Add(inputMesh);

			// if (output == null || output.mesh == null)
				// output = new MixtureMesh{ mesh = new Mesh() };

			// var instances = new CombineInstance[2];
			// instances[0].mesh = inputMesh.mesh;
			// instances[0].transform = inputMesh.localToWorld;
			// instances[1].mesh = output.mesh;
			// instances[1].transform = output.localToWorld;
			// output.mesh = new Mesh();
			// output.mesh.CombineMeshes(instances);
		}

		List<MixtureMesh> inputMeshes = new List<MixtureMesh>();
		public void PrepareNewIteration()
		{
			output = new MixtureMesh();
			inputMeshes.Clear();
		}

		public void FinalIteration()
		{
			var combineInstances = inputMeshes
				.Where(m => m?.mesh != null && m.mesh.vertexCount > 0)
				.Select(m => new CombineInstance{ mesh = m.mesh, transform = m.localToWorld })
				.ToArray();
			
			output.mesh = new Mesh { indexFormat = IndexFormat.UInt32};
			output.mesh.CombineMeshes(combineInstances);
		}
    }
}