using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
    [System.Serializable, NodeMenuItem("Utils/Texture 2D")]
    public class TextureNode : FixedShaderNode
    {
        public override string name => "Texture2D";

        public override string ShaderName => "Hidden/Mixture/Texture";

        public override bool displayMaterialInspector => true;

    }
}