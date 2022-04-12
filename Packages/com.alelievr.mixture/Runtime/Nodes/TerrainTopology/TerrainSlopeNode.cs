using System.Collections.Generic;
using System.Linq;
using GraphProcessor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Mixture
{
    [Documentation(@"
The slope map describes the steepness of the terrain and is handy for texturing the terrain.
")]
    [System.Serializable, NodeMenuItem("Terrain Topology/Slope")]
    public class TerrainSlopeNode : TerrainTopologyNode
    {
        //[Input][ShowAsDrawer] public float terrainHeight = 600; 
        public override string name => "Terrain Slope Map";
        public override bool isRenamable => true;
        protected override string KernelName => "Slope";
        
        public VisualizeMode _visualizeMode = VisualizeMode.GREYSCALE;
        public override VisualizeMode visualizeMode => _visualizeMode; 
        
        protected override bool ProcessNode(CommandBuffer cmd)
        {
            if (!base.ProcessNode(cmd))
                return false;
            cmd.SetComputeFloatParam(computeShader, "_TerrainHeight", terrainHeight);
            DispatchCompute(cmd, kernel, settings.GetResolvedWidth(graph), settings.GetResolvedHeight(graph));
            
            return true;
        }
    }
}