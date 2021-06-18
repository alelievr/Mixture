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
        }

		const int					k_HeightmapTileSize = 256;

		public Texture2D			savedHeightmap;
		public CustomRenderTexture	previewHeightmap;

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

			return true;
		}

		public bool IsOutOfBounds(Vector3Int index)
		{
			// x is the zoom
			if (index.x < 0 || index.x > 15)
				return true;
			
			if (index.y < 0 || index.z < 0)
				return true;
			
			int max = (int)Mathf.Pow(2, index.x);
			if (index.y >= max || index.z >= max)
				return true;
			
			return false;
		}

		public IEnumerable<HeightmapTile> GetVisibleTiles()
		{
			yield break;
			// int zoom = Mathf.RoundToInt(zoomLevel);

			// Vector2 min = 
			// // Limit index
			// if (node.IsOutOfBounds(index))
			// {
			// 	Debug.Log("Index out of data set: " + index);
			// 	return;
			// }
		}

        protected override void Disable()
		{
			CoreUtils.Destroy(previewHeightmap);
			base.Disable();
		}
    }
}