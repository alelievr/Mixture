using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using UnityEngine.Rendering;

namespace Mixture
{
    [System.Serializable, NodeMenuItem("Utils/Make Vector")]
    public class MakeVectorNode : MixtureNode, INeedsCPU
    {
        [Input("X")]
        public float X;
        [Input("Y")]
        public float Y;
        [Input("Z")]
        public float Z;
        [Input("W")]
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