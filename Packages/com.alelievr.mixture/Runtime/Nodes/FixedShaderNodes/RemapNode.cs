using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
    [System.Serializable, NodeMenuItem("Color/Remap")]
    public class RemapNode : FixedShaderNode
    {
        public override string name => "Remap";

        public override string shaderName => "Hidden/Mixture/Remap";

        public override bool displayMaterialInspector => true;
        protected override IEnumerable<string> filteredOutProperties => new string[] { "_Mode" };

    }
}