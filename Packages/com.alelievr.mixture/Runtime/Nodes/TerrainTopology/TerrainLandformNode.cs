using System.Collections.Generic;
using GraphProcessor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Mixture
{
    [Documentation(@"
The landform map combines some of the curvature values to try and classify the type of landform. For example if the landform is convex/concave or accumulative/dispersive.

The idea is the the shape of the landform determines how water flows over it which is a key indicator for soil type and depth.
")]
    [System.Serializable, NodeMenuItem("Terrain Topology/Landform")]
    public class TerrainLandformNode : TerrainTopologyNode
    {
        protected override string KernelName => "Landform";
        
        public override string name => "Terrain Landform Map";
        public override bool isRenamable => true;
        public VisualizeMode _visualizeMode = VisualizeMode.COLOR;
        public override VisualizeMode visualizeMode => _visualizeMode; 
        
        
        public enum LandformType
        {
            GAUSSIAN,
            SHAPE_INDEX,
            ACCUMULATION
        }

        [ShowInInspector(true)][SerializeField] public LandformType mode;
        public override bool showDefaultInspector => true;

        private Dictionary<LandformType, string> keywordMap = new Dictionary<LandformType, string>()
        {
            { LandformType.GAUSSIAN, "LANDFORM_GAUSSIAN" },
            { LandformType.SHAPE_INDEX, "LANDFORM_SHAPE_INDEX" },
            { LandformType.ACCUMULATION, "LANDFORM_ACCUMULATION" },
        };

        void SetKeyword()
        {
            foreach (var item in keywordMap)
            {
                if (mode == item.Key)
                {
                    computeShader.EnableKeyword(item.Value);
                }
                else
                {
                    computeShader.DisableKeyword(item.Value);
                }
            }
        }

        protected override bool ProcessNode(CommandBuffer cmd)
        {
            if (!base.ProcessNode(cmd))
                return false;
            SetKeyword();
            DispatchCompute(cmd, kernel, output.width, output.height);
            UpdateTempRenderTexture(ref output);
            return true;
        }
    }
}