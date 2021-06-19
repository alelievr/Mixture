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

		EarthHeightmap node;
		Dictionary<HeightmapTile, Texture2D> cache = new Dictionary<HeightmapTile, Texture2D>();
		List<(HeightmapTile tile, UnityWebRequest request)> requests = new List<(HeightmapTile, UnityWebRequest)>();
		List<HeightmapTile> failedRequestLocations = new List<HeightmapTile>();
		Vector2 mousePosition;
		VisualElement nodeContainer;

		public override void Enable(bool fromInspector)
		{
			base.Enable(fromInspector);

			node = nodeTarget as EarthHeightmap;

            controlsContainer.Add(new Label("Hello World"));

			if (!fromInspector)
			{
				schedule.Execute(UpdateEarthMap).Every(17);
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
					CoreUtils.Destroy(kp.Value);
				cache.Clear();
			}
		}

		void UpdateEarthMap()
		{
			// TODO: check if the node is in find map mode

			var cmd = new CommandBuffer{ name = "Update Earth Heightmap View" };
			cmd.SetRenderTarget(node.previewHeightmap);
			cmd.ClearRenderTarget(false, true, Color.clear);
			var props = new MaterialPropertyBlock();
			var material = node.GetTempMaterial("Hidden/Mixture/EarthHeightmap");

			foreach (var tile in node.GetVisibleTiles())
			{
				var tileTexture = LoadTile(tile);

				if (tileTexture != null)
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

					props.SetTexture("_Heightmap", tileTexture);
					props.SetVector("_DestinationOffset", offset);
					props.SetVector("_DestinationScale", scale);

					cmd.DrawProcedural(Matrix4x4.identity, material, 0, MeshTopology.Quads, 4, 1, props);
				}
			}

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

		Texture2D LoadTile(HeightmapTile tile)
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
						cache[request.tile] = DownloadHandlerTexture.GetContent(request.request);
						var filePath = GetCachePath(request.tile);
						File.WriteAllBytes(filePath, cache[request.tile].EncodeToPNG());
					}
					else
					{
						failedRequestLocations.Add(request.tile);
						Debug.LogError("Could not fetch earth height texture: " + request.request.error);
					}
					requests.Remove(request);
				}
			}

			if (cache.TryGetValue(tile, out var heightmap))
			{
				return heightmap;
			}
			else if (!requests.Any(r => r.tile.Equals(tile)))
			{
				// Avoid web requests with local cache
				string cachedFilePath = GetCachePath(tile);
				if (File.Exists(cachedFilePath))
				{
					var textureData = File.ReadAllBytes(cachedFilePath);
					heightmap = new Texture2D(1, 1);
					heightmap.LoadImage(textureData);
					cache[tile] = heightmap;
					// TODO: compute the min and max height in this tile
					return heightmap;
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