using System.Collections.Generic;
using GraphProcessor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Mixture
{
    [Documentation(@"
The residual map takes the heights around a point in a given window and performs some sort of statistical analysis. For example the standard deviation represents the roughness of the terrain.

Options to calculate the mean, standard deviation, percentile and a few others are provided.
")]
    [System.Serializable, NodeMenuItem("Terrain Topology/Residual")]
    public class TerrainResidualNode : TerrainTopologyNode
    {
        public override string name => "Terrain Residual Map";
        public override bool isRenamable => true;
        protected override string KernelName => "Residual";
        public VisualizeMode _visualizeMode;

        public override VisualizeMode visualizeMode => _visualizeMode;

        
        public enum ResidualType
        {
            ELEVATION,
            MEAN,
            DIFFERENCE,
            STDEV,
            DEVIATION,
            PERCENTILE
        }

        [ShowInInspector(true)][SerializeField] public ResidualType mode;
        public override bool showDefaultInspector => true;

        private Dictionary<ResidualType, string> keywordMap = new Dictionary<ResidualType, string>()
        {
            { ResidualType.ELEVATION, "RESIDUAL_ELEVATION" },
            { ResidualType.MEAN, "RESIDUAL_MEAN" },
            { ResidualType.DIFFERENCE, "RESIDUAL_DIFFERENCE" },
            { ResidualType.STDEV, "RESIDUAL_STDEV" },
            { ResidualType.DEVIATION, "RESIDUAL_DEVIATION" },
            { ResidualType.PERCENTILE, "RESIDUAL_PERCENTILE" },
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