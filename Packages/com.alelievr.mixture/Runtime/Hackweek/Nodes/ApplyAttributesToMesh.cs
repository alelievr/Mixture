using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using UnityEngine.Rendering;
using System;
using UnityEngine.Experimental.Rendering;
using Unity.Collections;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Apply Attributes To Mesh")]
	public class ApplyAttributesToMesh : MixtureNode
	{
		[Input("Mesh")]
		public MixtureMesh input;

        [Input("Attributes")]
        public MixtureAttributeList inputPoints;

		[Output("Mesh")]
		public MixtureMesh output;

		public override string	name => "Apply Attributes To Mesh";

		public override bool hasPreview => false;
		public override bool showDefaultInspector => true;

		protected override bool ProcessNode(CommandBuffer cmd)
		{
            if (input?.mesh == null || inputPoints == null)
                return false;

            Color[] colors = new Color[input.mesh.vertices.Length];
            
            foreach (var p in inputPoints)
            {
                if (!p.ContainsKey("index"))
                    continue;

                p.TryGetValue("index", out var index);
                int i = (int)index;
                if (p.TryGetValue("position", out var position) && position is Vector3 pos)
                    input.mesh.vertices[i] = pos;
                if (p.TryGetValue("normal", out var normal) && normal is Vector3 n)
                    input.mesh.normals[i] = n;
                if (p.TryGetValue("color", out var color) && color is Color c)
                    colors[i] = c;
                if (p.TryGetValue("uv", out var uv) && uv is Vector2 u)
                    input.mesh.uv[i] = u;
            }
        
            input.mesh.colors = colors;
            input.mesh.UploadMeshData(false);
            input.mesh.RecalculateBounds();

            output = input.Clone();

			return true;
		}
    }
}