using GraphProcessor;

namespace Mixture
{
    [System.Serializable, NodeMenuItem("Operator/Edge Tengent Flow")]
    public class EdgeTengentFlow : FixedShaderNode
    {
        public override string name => "Edge Tengent Flow";
        public override string shaderName => "Hidden/Mixture/EdgeTengentFlow";
        public override bool displayMaterialInspector => true;
    }
}