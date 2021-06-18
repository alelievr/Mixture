using UnityEngine;
using GraphProcessor;
using UnityEngine.Rendering;
using System.Collections.Generic;
using UnityEngine.Experimental.Rendering;
using System;

namespace Mixture
{
	[Documentation(@"
Retrieves the heightmap data of earth. This node is using the mapzen dataset, to learn more about it check out this page: https://www.mapzen.com/blog/elevation/.
")]

	[System.Serializable, NodeMenuItem("Utils/Earth Heightmap")]
	public class EarthHeightmap : MixtureNode
	{
		public struct HeightmapTile : IEquatable<HeightmapTile>
		{
			public int arrayIndex;
			public int zoom;
			public int x;
			public int y;

            public bool Equals(HeightmapTile other)
				=> zoom == other.zoom && x == other.x && y == other.y;

            public override string ToString() => $"({zoom}, {x}, {y})";
        }

		public const int			k_HeightmapTileSize = 256;
		public const int			k_MinZoom = 0;
		public const int			k_MaxZoom = 15;

		public Texture2D			savedHeightmap;
		public CustomRenderTexture	previewHeightmap;

		[Output("Heightmap")]
		public Texture				output;

		// TODO: replace that with float zoom + interpolation
		public float				zoomLevel = 0;
		public Vector2				center = Vector2.zero;

		[NonSerialized]
		List<HeightmapTile>			loadedTiles = new List<HeightmapTile>();

		public override string	name => "Earth Heightmap";

		public override Texture previewTexture => previewHeightmap;
		public override bool showDefaultInspector => true;
		protected override MixtureSettings defaultSettings => Get2DOnlyRTSettings(base.defaultSettings);
		public override PreviewChannels	defaultPreviewChannels => PreviewChannels.RGB;

		// Override this if you node is not compatible with all dimensions
		public override List<OutputDimension> supportedDimensions => new List<OutputDimension>() {
			OutputDimension.Texture2D,
		};

		protected override void Enable()
		{
			base.Enable();
			UpdateTempRenderTexture(ref previewHeightmap);
			// if (!graph.IsObjectInGraph(savedHeightmap))
			// 	graph.AddObjectToGraph(savedHeightmap);
		}

		protected override bool ProcessNode(CommandBuffer cmd)
		{
            if (!base.ProcessNode(cmd) || savedHeightmap == null)
                return false;

            // Update temp target in case settings changes
			UpdateTempRenderTexture(ref previewHeightmap);

			var heightmapConvertMaterial = GetTempMaterial("Hidden/Mixture/EarthHeightmap");
			heightmapConvertMaterial.SetTexture("_Heightmap", savedHeightmap);
			previewHeightmap.material = heightmapConvertMaterial;
			CustomTextureManager.UpdateCustomRenderTexture(cmd, previewHeightmap, 1);

			// TODO: check preview mode
			output = previewHeightmap;

			return true;
		}

		public bool IsOutOfBounds(HeightmapTile tile)
		{
			// x is the zoom
			if (tile.zoom < 0 || tile.zoom > 15)
				return true;

			if (tile.x < 0 || tile.y < 0)
				return true;

			int max = (int)Mathf.Pow(2, tile.zoom);
			if (tile.x >= max || tile.y >= max)
				return true;
			
			return false;
		}

		public HeightmapTile LocalToWorld(Rect visibleArea)
		{
			float zoomLevel = Mathf.Log(1.0f / visibleArea.width, 2);
			int zoom = Mathf.RoundToInt(zoomLevel);
			int tileMax = (int)Mathf.Pow(2, zoom);

			return new HeightmapTile
			{
				zoom = zoom,
				x = Mathf.FloorToInt(tileMax * (visibleArea.x / 2.0f + 0.5f)),
				y = Mathf.FloorToInt(tileMax * (visibleArea.y / 2.0f + 0.5f)),
			};
		}

		public Rect WorldToLocal(HeightmapTile worldPos)
		{
			float size = 1 / Mathf.Pow(2, worldPos.zoom);
			int tileMax = (int)Mathf.Pow(2, worldPos.zoom);

			float x = worldPos.x / (float)tileMax * 2.0f - 1.0f;
			float y = worldPos.y / (float)tileMax * 2.0f - 1.0f;

			return new Rect(x, y, size, size);
		}

		public IEnumerable<HeightmapTile> GetVisibleTiles()
		{
			// calculate visible rect between -1 and 1
			float size = 1.0f / Mathf.Pow(2, zoomLevel);
			Rect visibleArea = new Rect(-center.x, -center.y, size, size);

			// Calculate all visible tiles from the visible area:
			Vector2 resolution = new Vector2(settings.GetResolvedWidth(graph), settings.GetResolvedHeight(graph));
			Vector2Int tileCount = new Vector2Int(Mathf.CeilToInt(resolution.x / 256.0f), Mathf.CeilToInt(resolution.y / 256.0f));

			Vector2 offset = visibleArea.position - new Vector2(size, size) / 2.0f;
			float tileSize = Mathf.Min(size / tileCount.x, size / tileCount.y);
			var minTile = LocalToWorld(new Rect(offset.x, offset.y, tileSize, tileSize));
			for (int x = -1; x <= tileCount.x; x++)
			{
				for (int y = -1; y <= tileCount.y; y++)
				{
					// Rect tileArea = new Rect(x * tileSize + offset.x, y * tileSize + offset.y, tileSize, tileSize);
					// HeightmapTile tile = LocalToWorld(tileArea);
					var tile = new HeightmapTile{ x = minTile.x + x, y = minTile.y + y, zoom = minTile.zoom };
					Debug.Log("Loading tile area: " + tile);

					if (IsOutOfBounds(tile))
						continue;

					yield return tile;
				}
			}
		}

        protected override void Disable()
		{
			CoreUtils.Destroy(previewHeightmap);
			base.Disable();
		}
    }
}