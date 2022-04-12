using GraphProcessor;
using UnityEngine.Rendering;

namespace Mixture
{
    [Documentation(@"Represent the horizontal gradient of the terrain")]
    [System.Serializable, NodeMenuItem("Terrain Topology/Aspect")]
    public class TerrainAspectNode : TerrainTopologyNode
    {
        public override string name => "Terrain Aspect Map";
        public override bool isRenamable => true;
        protected override string KernelName => "Aspect";

        public override VisualizeMode visualizeMode => VisualizeMode.COLOR;

        protected override bool ProcessNode(CommandBuffer cmd)
        {
            if (!base.ProcessNode(cmd))
                return false;
            
            DispatchCompute(cmd, kernel, output.width, output.height);
            UpdateTempRenderTexture(ref output);
            return true;
        }
    }
}