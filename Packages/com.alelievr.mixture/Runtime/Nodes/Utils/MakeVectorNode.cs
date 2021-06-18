using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using UnityEngine.Rendering;

namespace Mixture
{
    [Documentation(@"
Combine up to 4 float values into a Vector4.
")]

    [System.Serializable, NodeMenuItem("Math/Make Vector")]
    public class MakeVectorNode : MixtureNode
    {
		public override bool showDefaultInspector => true;
		public override float nodeWidth => MixtureUtils.smallNodeWidth;

        [Input("X"), ShowAsDrawer]
        public float X;
        [Input("Y"), ShowAsDrawer]
        public float Y;
        [Input("Z"), ShowAsDrawer]
        public float Z;
        [Input("W"), ShowAsDrawer]
        public float W;

        [Output("Vector")]
        public Vector4 vector;

        public override string name => "Make Vector";

        protected override bool ProcessNode(CommandBuffer cmd)
        {
            vector = new Vector4(X, Y, Z, W);
            return true;
        }
    }
}