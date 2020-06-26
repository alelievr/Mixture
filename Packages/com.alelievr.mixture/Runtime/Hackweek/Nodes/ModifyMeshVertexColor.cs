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
	[System.Serializable, NodeMenuItem("Modify Mesh Vertex Color")]
	public class ModifyMeshVertexColor : MixtureNode
	{
        [Input("Attributes")]
        public MixtureAttributeList inputPoints;

        [Input("Mesh Attribute")]
        public MixtureAttribute     meshAttrib;
        
        [Output("Attributes")]
        public MixtureAttributeList outputPoints;

		public override string	name => "Modify Mesh Vertex Color";

		public override bool hasPreview => false;
		public override bool showDefaultInspector => true;

		protected override bool ProcessNode(CommandBuffer cmd)
		{
            if (inputPoints == null || meshAttrib == null)
                return false;

             outputPoints = inputPoints;

            meshAttrib.TryGetValue("scale", out var meshScale);
            Vector3 meshScaleVec = (Vector3)meshScale;

            foreach (var point in inputPoints)
            {
                // We need the position in this node
                if (!point.ContainsKey("position"))
                    continue;

                var pos = (Vector3)point["position"];

                point["color"] = new Color(meshScaleVec.x, meshScaleVec.y, meshScaleVec.z);
            }
        
			return true;
		}
    }
}