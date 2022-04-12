using UnityEngine;

namespace Mixture
{
    [System.Serializable]
    public class MixtureTerrain
    {
        private TerrainData _terrainData;
        public TerrainData TerrainData => _terrainData;

        public MixtureTerrain(TerrainData terrainData)
        {
            this._terrainData = terrainData;
        }

        public Vector2Int HeightMapResolution =>
            new Vector2Int(_terrainData.heightmapResolution, _terrainData.heightmapResolution);

        public Vector2 Dimension => new Vector2(_terrainData.size.x, _terrainData.size.z);
        public float Height => _terrainData.size.y;

        public Vector2 CellSize =>
            new Vector2(HeightMapResolution.x / Dimension.x, HeightMapResolution.y / Dimension.y);

        public RenderTexture Heightmap => _terrainData.heightmapTexture;
    }
}