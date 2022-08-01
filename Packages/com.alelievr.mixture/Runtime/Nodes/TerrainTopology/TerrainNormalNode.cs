using GraphProcessor;
using UnityEngine.Rendering;

namespace Mixture
{
    [Documentation(@"
Compute the normal of the terrain. The normal represent the direction of the surface in tangent space
")]
    [System.Serializable, NodeMenuItem("Terrain Topology/Normal")]
    public class TerrainNormalNode : TerrainTopologyNode
    {
        public override string name => "Terrain Normal Map";
        public override bool isRenamable => true;
        protected override string KernelName => "Normal";
        
        
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