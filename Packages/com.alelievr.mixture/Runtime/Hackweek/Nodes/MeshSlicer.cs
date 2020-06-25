using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using UnityEngine.Rendering;
using System;
using System.Diagnostics;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Mesh Slicer")]
	public class MeshSlicer : MixtureNode
	{
		[Input("Mesh")]
		public MixtureMesh input;

        [Input("Input Planes")]
        public MixtureAttributeList inputPlanes;

        public Vector3 planePos;
        public Vector3 planeNormal = Vector3.up;
        public float uvScale = 1;

		[Output("Sliced Mesh")]
		public MixtureMesh output;
		public override string	name => "Mesh Slicer";
		public override bool hasPreview => true;
		public override bool showDefaultInspector => true;
		// public Vector3 scale = Vector3.one;
		// There is an issue with json serialization and new keyword :) 
		// public Vector3 bug_position = Vector3.zero;
        public override Texture previewTexture => output?.mesh != null && !MixtureGraphProcessor.isProcessing ? UnityEditor.AssetPreview.GetAssetPreview(output.mesh) ?? Texture2D.blackTexture : Texture2D.blackTexture;

        public bool positive;

        MeshCutter cutter;
        private Plane slicePlane = new Plane();

        protected override void Enable()
        {
            cutter = new MeshCutter(128);
        }

		protected override bool ProcessNode(CommandBuffer cmd)
		{
			if (input == null)
				return false;

            Mesh outputMesh = new Mesh { indexFormat = IndexFormat.UInt32};
            bool success = false;

            if (inputPlanes == null || inputPlanes.Count == 0)
            {
                slicePlane.SetNormalAndPosition(planeNormal, planePos + Vector3.up * 0.00001f);
                try
                {
                    success = cutter.SliceMesh(input.mesh, ref slicePlane, uvScale);
                    var t = positive ? cutter.PositiveMesh : cutter.NegativeMesh;
                    outputMesh.SetVertices(t.vertices);
                    outputMesh.SetTriangles(t.triangles, 0);
                    outputMesh.SetNormals(t.normals);
                    outputMesh.SetUVs(0, t.uvs);
                } catch {}
                if (success == false)
                    outputMesh = UnityEngine.Object.Instantiate(input.mesh);
            }
            else
            {
                outputMesh = UnityEngine.Object.Instantiate(input.mesh);
                foreach (var attr in inputPlanes)
                {
                    if (attr.TryGetValue("position", out var pos) && attr.TryGetValue("normal", out var normal))
                    {
                        if (pos is Vector3 p && normal is Vector3 n)
                        {
                            try {
                                slicePlane.SetNormalAndPosition(n, p);
                                success = cutter.SliceMesh(outputMesh, ref slicePlane, uvScale);
                                var t = positive ? cutter.PositiveMesh : cutter.NegativeMesh;
                                outputMesh.SetVertices(t.vertices);
                                outputMesh.SetTriangles(t.triangles, 0);
                                outputMesh.SetNormals(t.normals);
                                outputMesh.SetUVs(0, t.uvs);
                            } catch { success = false; }
                        }
                    }
                }
            }

            output = new MixtureMesh{ mesh = outputMesh };
			return true;
		}
    }
}