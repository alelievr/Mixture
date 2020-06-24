using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using UnityEngine.Rendering;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Transform Mesh")]
	public class TransformMesh : MixtureNode
	{
		[Input("Mesh")]
		public MixtureMesh inputMesh;
		[Input("Attribute")]
		public MixtureAttribute inputAttrib;

        public Vector3 pos;
        public Vector3 eulerAngles;
        public Vector3 scale = Vector3.one;
        
        [Output("Output")]
        public MixtureMesh output;

		public override string	name => "Transform Mesh";

		public override Texture previewTexture => output?.mesh != null && !MixtureGraphProcessor.isProcessing ? UnityEditor.AssetPreview.GetAssetPreview(output.mesh) ?? Texture2D.blackTexture : Texture2D.blackTexture;
		public override bool    hasPreview => true;
		public override bool    showDefaultInspector => true;

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
            if (inputMesh == null || inputMesh.mesh == null)
                return false;

			output = inputMesh.Clone();

            if (inputAttrib != null)
            {
                // Try to get values from attribute in param:
                inputAttrib.TryGetValue("position", out var position);
                inputAttrib.TryGetValue("rotation", out var rotation);
                inputAttrib.TryGetValue("scale", out var scale);
                inputAttrib.TryGetValue("normal", out var normal);

                if (normal != null && normal is Vector3 n)
                    rotation = Quaternion.LookRotation(n, Vector3.up) * Quaternion.Euler(90, 0, 0);
                if (position == null)
                    position = Vector3.zero;
                if (rotation == null)
                    rotation = Quaternion.identity;
                if (scale == null)
                    scale = Vector3.one;
                
                // float4x4 m1 = output.localToWorld;
                // float4x4 m2 = new float4x4((quaternion)(Quaternion)rotation, (float3)(Vector3)position);
                // Vector3 s = (Vector3)scale;
                // // m2.c0.x *= s.x;
                // // m2.c1.y *= s.y;
                // // m2.c2.z *= s.z;
                // m2 *= m1;
                // output.localToWorld = transpose(m2); 

                output.localToWorld *= Matrix4x4.TRS((Vector3)position, (Quaternion)rotation, (Vector3)scale);
                // output.localToWorld = MultiplyMatrix(output.localToWorld, Matrix4x4.TRS((Vector3)position, (Quaternion)rotation, (Vector3)scale));

                // Is this needed ?
                // Solid s = new Solid(output.mesh.vertices, output.mesh.triangles, output.mesh.normals);
                // s.ApplyMatrix(output.localToWorld);
            }
            else
            {
                output.localToWorld = Matrix4x4.TRS(pos, Quaternion.Euler(eulerAngles), scale);
            }

            var combine = new CombineInstance[1];
            combine[0].mesh = output.mesh;
            combine[0].transform = output.localToWorld;

            output.mesh = new Mesh{ indexFormat = IndexFormat.UInt32};
            output.localToWorld = Matrix4x4.identity;
            output.mesh.CombineMeshes(combine);

			return true;
		}
    }
}