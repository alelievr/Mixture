using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
    [System.Serializable, NodeMenuItem("Matte/Texture")]
    public class TextureNode : FixedShaderNode
    {
        public override string name => "Texture Matte";

        public override string shaderName => "Hidden/Mixture/Texture";

        public override bool displayMaterialInspector => true;
    }
}