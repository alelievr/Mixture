using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
    [System.Serializable, NodeMenuItem("Math/Scale & Bias")]
    public class ScaleBiasNode : FixedShaderNode
    {
        public override string name => "Scale & Bias";


        public override string shaderName => "Hidden/Mixture/ScaleBias";

        public override bool displayMaterialInspector => true;

        public override Precision precision => Precision.Float;
    }
}