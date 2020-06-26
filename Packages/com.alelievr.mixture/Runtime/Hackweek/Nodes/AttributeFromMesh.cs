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
	[System.Serializable, NodeMenuItem("Attribute From Mesh")]
	public class AttributeFromMesh : MixtureNode
	{
		[Input("Mesh")]
		public MixtureMesh input;

        [Output("Output")]
        public MixtureAttributeList outputPoints;

		public override string	name => "Attribute From Mesh";

		public override bool hasPreview => false;
		public override bool showDefaultInspector => true;

		protected override bool ProcessNode(CommandBuffer cmd)
		{
            if (input?.mesh == null)
                return false;
            
            outputPoints = new MixtureAttributeList();

            for (int i = 0; i < input.mesh.vertices.Length; i++)
            {
                Color c = (input.mesh.colors.Length > i) ? input.mesh.colors[i] : Color.black;
                Vector2 uv = (input.mesh.uv.Length > i) ? input.mesh.uv[i] : Vector2.zero;
                outputPoints.Add(new MixtureAttribute{
                    { "index", i},
                    { "position", input.mesh.vertices[i] },
                    { "normal", input.mesh.normals[i] },
                    { "color", c },
                    { "uv", uv },
                });
            }

			return true;
		}
    }
}