using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
    [Documentation(@"
Remap a texture using another texture as map. You can choose to remap either from the Hue, Saturation, Value(brightness) or alpha of the input texture.

Note that for the map texture, you only need a texture of 1 pixel height, thus you can use the Gradient node as a remap value.
")]

    [System.Serializable, NodeMenuItem("Color/Remap")]
    public class RemapNode : FixedShaderNode
    {
        public override string name => "Remap";

        public override string shaderName => "Hidden/Mixture/Remap";

        public override bool displayMaterialInspector => true;
        protected override IEnumerable<string> filteredOutProperties => new string[] { "_Mode" };

    }
}