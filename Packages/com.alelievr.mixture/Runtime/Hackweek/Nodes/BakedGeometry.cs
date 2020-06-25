using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using UnityEngine.Rendering;
using System;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Baked Geometry")]
	public class BakedGeometry : MixtureNode
	{
		[Input("Input Meshes")]
		public MixtureMesh inputMesh;
		// [Input(name = "Positions")]
		// public List< Vector3 >		positions = new List<Vector3>();
		// [Input(name = "Triangles")]
		// public List< int >			triangles = new List<int>();
		// [Input(name = "Normals")]
		// public List< Vector3 >		normals = new List<Vector3>();
		// [Input(name = "Tangents")]
		// public List< Vector3 >		tangent = new List<Vector3>();
		// [Input(name = "UV 0")]
		// public List< Vector4 >		uv0 = new List<Vector4>();

		// TODO: other stuff

		public Mesh				mesh = new Mesh { name = "Baked Geometry", indexFormat = IndexFormat.UInt32};
		public override string	name => "Baked Geometry";

		public override Texture previewTexture => !MixtureGraphProcessor.isProcessing ? UnityEditor.AssetPreview.GetAssetPreview(mesh) ?? (Texture)preview : Texture2D.blackTexture;
		CustomRenderTexture		preview;

		protected override void Enable()
		{
			UpdateTempRenderTexture(ref preview);

			// Update temp RT after process in case RTSettings have been modified in Process()
			afterProcessCleanup += () => {
				UpdateTempRenderTexture(ref preview);
			};
		}

        protected override void Disable() => CoreUtils.Destroy(preview);

		// // Functions with Attributes must be either protected or public otherwise they can't be accessed by the reflection code
		// [CustomPortBehavior(nameof(materialInputs))]
		// public IEnumerable< PortData > ListMaterialProperties(List< SerializableEdge > edges)
		// {
		// 	foreach (var p in GetMaterialPortDatas(material))
		// 	{
		// 		if (filteredOutProperties.Contains(p.identifier))
		// 			continue;
		// 		yield return p;
		// 	}
		// }

		// [CustomPortInput(nameof(materialInputs), typeof(object))]
		// protected void GetMaterialInputs(List< SerializableEdge > edges)
		// {
		// 	AssignMaterialPropertiesFromEdges(edges, material);
		// }

		protected override bool ProcessNode(CommandBuffer cmd)
		{
			if (!graph.IsObjectInGraph(mesh))
			{
				graph.AddObjectToGraph(mesh);
			}

			if (inputMesh == null || inputMesh.mesh == null)
				return false;
			

			// Copy input mesh to output to avoid breaking the external ref:
			var m = inputMesh.mesh;
			mesh.Clear();
			mesh.vertices = m.vertices;
			mesh.triangles = m.triangles;
			mesh.normals = m.normals;
			mesh.tangents = m.tangents;
			mesh.colors = m.colors;
			mesh.uv = m.uv;
			mesh.uv2 = m.uv2;
			mesh.uv3 = m.uv3;
			mesh.uv4 = m.uv4;
			mesh.uv5 = m.uv5;
			mesh.uv6 = m.uv6;
			mesh.uv7 = m.uv7;
			mesh.uv8 = m.uv8;
			mesh.bounds = m.bounds;
			mesh.bindposes = m.bindposes;
			mesh.boneWeights = m.boneWeights;

			return true;
		}
    }
}