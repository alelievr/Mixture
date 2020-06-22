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

		public void Reset()
		{
			// ??
		}

		protected override bool ProcessNode(CommandBuffer cmd)
		{
			output = inputMesh;

			switch (mode)
			{
				case AggregationMode.MergeResult:
					// TODO Mesh.CombineMeshes
					break;
				default:
				case AggregationMode.None:
					break;
			}
			return true;
		}
    }
}