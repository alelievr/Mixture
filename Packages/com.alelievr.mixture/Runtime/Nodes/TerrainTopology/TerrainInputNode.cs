using GraphProcessor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Mixture
{
    [Documentation(@"
Helper node able to input a TerrainData in the graph. It could be used with all Terrain Topology Node
to input the terrain dimension and terrain height
")]
    [System.Serializable, NodeMenuItem("Terrain/Terrain Input")]
    public class TerrainInputNode : MixtureNode, ICreateNodeFrom<TerrainData>, ICreateNodeFrom<Terrain>
    {
        [Output("Terrain Data")] public MixtureTerrain output;
        [ShowInInspector(true)] public TerrainData _terrainData;
        public override string name => "Terrain Input";
        public override bool hasPreview => false;
        public override bool isRenamable => true;
        public override bool showDefaultInspector => true;

        protected override bool ProcessNode(CommandBuffer cmd)
        {
            if (!base.ProcessNode(cmd))
            {
                return false;
            }

            if (this._terrainData == null)
            {
                this.output = null;
                return false;
            }

            this.output = new MixtureTerrain(_terrainData);
            return true;
        }

        public bool InitializeNodeFromObject(TerrainData value)
        {
            _terrainData = value;
            return true;
        }

        public bool InitializeNodeFromObject(Terrain value)
        {
            this._terrainData = value.terrainData;
            return true;
        }
    }
}