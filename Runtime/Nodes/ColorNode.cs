using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
    [System.Serializable, NodeMenuItem("Matte/Color")]
    public class ColorNode : FixedShaderNode
    {
        public override string name => "Color Matte";

        public override string shaderName => "Hidden/Mixture/Color";

        public override bool displayMaterialInspector => true;
    }
}