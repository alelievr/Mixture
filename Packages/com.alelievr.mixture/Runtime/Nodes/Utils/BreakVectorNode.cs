using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
    [System.Serializable, NodeMenuItem("Utils/Break Vector")]
    public class BreakVectorNode : MixtureNode, INeedsCPU
    {
        [Input("Vector")]
        public Vector4 vector;

        [Output("X")]
        public float X;
        [Output("Y")]
        public float Y;
        [Output("Z")]
        public float Z;
        [Output("W")]
        public float W;

        public override string name => "Break Vector";

        protected override bool ProcessNode()
        {
            X = vector.x;
            Y = vector.y;
            Z = vector.z;
            W = vector.w;
            return true;
        }
    }
}