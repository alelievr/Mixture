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
			List,
			None,
		}

		[Input("Input")]
		public MixtureMesh inputMesh;

        [Output("Output")]
        public object output;

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

		[CustomPortBehavior(nameof(output))]
		protected IEnumerable< PortData > GetMaterialInputs(List< SerializableEdge > edges)
		{
			var portData = new PortData{
				identifier = nameof(output),
				displayName = "Output",
				acceptMultipleEdges = true,
			};
			switch (mode)
			{
				case AggregationMode.MergeResult:
					portData.displayType = typeof(MixtureMesh);
					break;
				case AggregationMode.List:
					portData.displayType = typeof(List<MixtureMesh>);
					break;
				default:
				case AggregationMode.None:
					portData.displayType = typeof(MixtureMesh);
					break;
			}

			yield return portData;
		}

		protected override bool ProcessNode(CommandBuffer cmd)
		{
			inputMeshes.Add(inputMesh);
			switch (mode)
			{
				case AggregationMode.MergeResult:
					break;
				default:
				case AggregationMode.None:
					output = inputMesh.Clone();
					break;
				case AggregationMode.List:
					output = inputMeshes;
					break;
			}
			return true;
		}

		List<MixtureMesh> inputMeshes = new List<MixtureMesh>();
		public void PrepareNewIteration()
		{
			output = new MixtureMesh();
			inputMeshes.Clear();
		}

		public void FinalIteration()
		{
			if (mode == AggregationMode.MergeResult || mode == AggregationMode.None)
			{
				var combineInstances = inputMeshes
					.Where(m => m?.mesh != null && m.mesh.vertexCount > 0)
					.Select(m => new CombineInstance{ mesh = m.mesh, transform = m.localToWorld })
					.ToArray();

				var mixtureMesh = output as MixtureMesh;
				mixtureMesh.mesh = new Mesh { indexFormat = IndexFormat.UInt32};
				mixtureMesh.mesh.CombineMeshes(combineInstances);
				output = mixtureMesh;
			}
		}
    }
}