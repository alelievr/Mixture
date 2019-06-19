using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
    [System.Serializable, NodeMenuItem("Matte/Gradient")]
    public class GradientNode : FixedShaderNode
    {
        public override string name => "Gradient";

        public override string shaderName => "Hidden/Mixture/Gradient";

        public override bool displayMaterialInspector => true;
    }
}