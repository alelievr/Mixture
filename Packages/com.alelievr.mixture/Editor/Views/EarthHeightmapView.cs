using System.Collections.Generic;
using UnityEngine.UIElements;
using GraphProcessor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Networking;
using System.Linq;
using System.IO;
using HeightmapTile = Mixture.EarthHeightmap.HeightmapTile;

namespace Mixture
{
	[NodeCustomEditor(typeof(EarthHeightmap))]
	public class EarthHeightmapView : MixtureNodeView
	{
		static readonly string endPoint = "https://s3.amazonaws.com/elevation-tiles-prod/terrarium/";
		static readonly string localCachePath = Application.temporaryCachePath;
		static readonly float zoomSpeed = 3;

		class HeightmapTileData
		{
			public Texture2D heightmap;
			public float minHeight;
			public float maxHeight;

			public HeightmapTileData(Texture2D heightmap)
			{
				this.heightmap = heightmap;
				minHeight = 1e20f;
				maxHeight = -1e20f;

				// Calculate the min and max height inside this tile:
				var pixels = heightmap.GetPixels32(0);
				for (int i = 0; i < pixels.Length; i++)
				{
					var heightColor = pixels[i];
					float height = heightColor.r * 256 + heightColor.g + (float)heightColor.b / 256.0f - 32768;
					minHeight = Mathf.Min(height, minHeight);
					maxHeight = Mathf.Max(height, maxHeight);
				}
			}
		}

		EarthHeightmap node;
		Dictionary<HeightmapTile, HeightmapTileData> cache = new Dictionary<HeightmapTile, HeightmapTileData>();
		List<(HeightmapTile tile, UnityWebRequest request)> requests = new List<(HeightmapTile, UnityWebRequest)>();
		List<HeightmapTile> failedRequestLocations = new List<HeightmapTile>();
		Vector2 mousePosition;
		VisualElement nodeContainer;

		public override void Enable(bool fromInspector)
		{
			base.Enable(fromInspector);

			node = nodeTarget as EarthHeightmap;

			if (!fromInspector)
			{
				var heightDataLabel = new Label();
				controlsContainer.Add(heightDataLabel);
				schedule.Execute(() => UpdateEarthMap(heightDataLabel)).Every(17);
				nodeContainer = controlsContainer;

				SetupPreviewEvents();
			}
			// TODO: add custom preview
		}

        public override void Disable()
		{
			if (nodeContainer == this.controlsContainer)
			{
				foreach (var kp in cache)
					if (kp.Value.heightmap != null)
						CoreUtils.Destroy(kp.Value.heightmap);
				cache.Clear();
			}
		}

		void UpdateEarthMap(Label heightDataLabel)
		{
			// TODO: check if the node is in find map mode

			var cmd = new CommandBuffer{ name = "Update Earth Heightmap View" };
			cmd.SetRenderTarget(node.previewHeightmap);
			cmd.ClearRenderTarget(false, true, Color.clear);
			var props = new MaterialPropertyBlock();
			var material = node.GetTempMaterial("Hidden/Mixture/EarthHeightmap");
			
			props.SetFloat("_MinHeight", node.minHeight);
			props.SetFloat("_MaxHeight", node.maxHeight);
			props.SetFloat("_Scale", 1.0f / node.inverseScale);
			props.SetFloat("_RemapMin", node.remapMin);
			props.SetFloat("_RemapMax", node.remapMax);
			props.SetFloat("_Mode", (int)node.mode);

			node.minHeight = 1e20f;
			node.maxHeight = -1e20f;
			foreach (var tile in node.GetVisibleTiles())
			{
				var tileData = LoadTile(tile);

				if (tileData?.heightmap != null)
				{
					// Calculate the position on screen based on tile coords:
					var local = node.WorldToLocal(tile);
					Vector2 scale = new Vector2(local.size.x, local.size.y);
					Vector2 offset = local.min;
					offset += node.center;
					offset *= Mathf.Pow(2, node.zoomLevel);
					scale *= Mathf.Pow(2, node.zoomLevel);

					// remap offset
					offset = offset * 0.5f + Vector2.one * 0.5f;

					props.SetTexture("_Heightmap", tileData.heightmap);
					props.SetVector("_DestinationOffset", offset);
					props.SetVector("_DestinationScale", scale);

					cmd.DrawProcedural(Matrix4x4.identity, material, 0, MeshTopology.Quads, 4, 1, props);

					// Update the node min and max height
					node.minHeight = Mathf.Min(node.minHeight, tileData.minHeight);
					node.maxHeight = Mathf.Max(node.maxHeight, tileData.maxHeight);
					node.savedHeightmap = tileData.heightmap;
				}
			}

			heightDataLabel.MarkDirtyRepaint();

			// TODO: auto remap mode
			if (node.mode == EarthHeightmap.HeightMode.Remap)
			{
				// TODO
			}
			else if (node.mode == EarthHeightmap.HeightMode.Scale)
			{
				node.minHeight *= 1.0f / node.inverseScale;
				node.maxHeight *= 1.0f / node.inverseScale;
			}

			node.minHeight += node.heightOffset;
			node.maxHeight += node.heightOffset;

			heightDataLabel.text = $" Height between {node.minHeight} and {node.maxHeight} meters";

			MarkDirtyRepaint();
			Graphics.ExecuteCommandBuffer(cmd);
		}

		void SetupPreviewEvents()
		{
			var preview = previewContainer.Q("ImGUIPreview");

			preview.RegisterCallback<WheelEvent>(e => {
				float cameraSizeBeforeZoom = 1.0f / Mathf.Pow(2, node.zoomLevel);
				node.zoomLevel += -e.delta.y / 100 * zoomSpeed;
				node.zoomLevel = Mathf.Clamp(node.zoomLevel, EarthHeightmap.k_MinZoom, EarthHeightmap.k_MaxZoom);

				float cameraSizeAfterZoom = 1.0f / Mathf.Pow(2, node.zoomLevel);
				float diff = (cameraSizeAfterZoom - cameraSizeBeforeZoom);
				var centeredMousePos = (mousePosition * 2.0f - Vector2.one);
				// Calculate mouse position on the map:
				Vector2 worldPos = node.center + centeredMousePos * cameraSizeAfterZoom;
				var movement = diff * centeredMousePos;
				node.center += movement;

				NotifyNodeChanged();
				e.StopImmediatePropagation();
			});

			preview.RegisterCallback<MouseMoveEvent>(e => {
				var localPos = GetPreviewMousePositionBetween01(e.mousePosition);
				mousePosition = localPos;

				if (e.imguiEvent.button == 1)
				{
					NotifyNodeChanged();

					// adjust movement ratio based on the screen size of the preview
					var delta = e.mouseDelta / preview.worldBound.size;
					// Scale the delta to it fits the -1 1 size of the canvas
					delta *= 2;

					node.center += delta / Mathf.Pow(2, node.zoomLevel);
				}
			});

			// Stop right click when the mouse is over the preview because we use it for moving the world pos
			previewContainer.RegisterCallback<ContextualMenuPopulateEvent>(e => {
				// TODO: check if the node is in preview mode
				e.StopImmediatePropagation();
			}, TrickleDown.TrickleDown);
			previewContainer.AddManipulator(new ContextualMenuManipulator(evt => {}));
		}

		HeightmapTileData LoadTile(HeightmapTile tile)
		{
			// Avoid spamming the API with invalid / wrong positions
			if (failedRequestLocations.Contains(tile))
				return null;

			// Update web requests
			foreach (var request in requests.ToList())
			{
				if (request.request.isDone)
				{
					if (request.request.result == UnityWebRequest.Result.Success)
					{
						var tileData = new HeightmapTileData(DownloadHandlerTexture.GetContent(request.request));
						tileData.heightmap.hideFlags = HideFlags.HideAndDontSave;
						cache[request.tile] = tileData;
						var filePath = GetCachePath(request.tile);
						File.WriteAllBytes(filePath, tileData.heightmap.EncodeToPNG());
					}
					else
					{
						failedRequestLocations.Add(request.tile);
						Debug.LogError("Could not fetch earth height texture: " + request.request.error);
					}
					requests.Remove(request);
				}
			}

			if (cache.TryGetValue(tile, out var heightmapData))
			{
				return heightmapData;
			}
			else if (!requests.Any(r => r.tile.Equals(tile)))
			{
				// Avoid web requests with local cache
				string cachedFilePath = GetCachePath(tile);
				if (File.Exists(cachedFilePath))
				{
					var textureData = File.ReadAllBytes(cachedFilePath);
					var heightmap = new Texture2D(1, 1);
					heightmap.hideFlags = HideFlags.HideAndDontSave;
					heightmap.LoadImage(textureData);
					cache[tile] = new HeightmapTileData(heightmap);
					return cache[tile];
				}
				else
				{
					Debug.Log("API CALL!");
					var request = UnityWebRequestTexture.GetTexture($"{endPoint}{tile.zoom}/{tile.x}/{tile.y}.png");
					request.SendWebRequest();
					requests.Add((tile, request));
				}
			}

			return null;
		}

		string GetCachePath(HeightmapTile tile)
			=> localCachePath + $"EarthHeightmap_{tile.zoom}_{tile.x}_{tile.y}";
	}
}