using UnityEngine.UIElements;
using GraphProcessor;
using UnityEngine.Rendering;
using UnityEngine;
using UnityEditor;

namespace Mixture
{
	[NodeCustomEditor(typeof(PreviewNode))]
	public class PreviewNodeView : FixedShaderNodeView 
	{
		PreviewNode		node;

		public override void Enable(bool fromInspector)
		{
			base.Enable(fromInspector);

			node = nodeTarget as PreviewNode;

			var colorPickerValues = new Label();
			colorPickerValues.AddToClassList("Indent");
			if (!fromInspector)
			{
				controlsContainer.Add(colorPickerValues);

				// Stop right click when the mouse is over the preview because we use it for light position
				previewContainer.RegisterCallback<ContextualMenuPopulateEvent>(e => {
					e.StopImmediatePropagation();
				}, TrickleDown.TrickleDown);
				previewContainer.AddManipulator(new ContextualMenuManipulator(evt => {}));
			}
			else
			{
				owner.graph.afterCommandBufferExecuted += UpdateViewData;
				controlsContainer.RegisterCallback<DetachFromPanelEvent>(e => {
					owner.graph.afterCommandBufferExecuted -= UpdateViewData;
				});
				var histogram = new HistogramView(node.histogramData, owner);
				controlsContainer.Add(histogram);
			}

			void UpdateViewData()
			{
				if (node.output != null)
				{
					// Update histogram
					var cmd = CommandBufferPool.Get("Update Histogram");
					HistogramUtility.ComputeHistogram(cmd, node.output, node.histogramData);
					Graphics.ExecuteCommandBuffer(cmd);

					// Update color picker data
					UpdateColorPickerValues();
				}
			}

			void UpdateColorPickerValues()
			{
				int texturePosX = Mathf.RoundToInt(node.mousePosition.x * node.output.width);
				int texturePosY = Mathf.RoundToInt(node.mousePosition.y * node.output.height);

				texturePosX = Mathf.Clamp(texturePosX, 0, node.output.width - 1);
				texturePosY = Mathf.Clamp(texturePosY, 0, node.output.height - 1);

				var a = AsyncGPUReadback.Request(node.output, 0, texturePosX, 1, texturePosY, 1, 0, 1, TextureFormat.RGBAFloat, (data) => {
					var colors = data.GetData<Color>();
					if (data.hasError || colors.Length == 0)
						return;
					var pixel = colors[0];
					colorPickerValues.text = $"R: {pixel.r:F3} G: {pixel.g:F3} B: {pixel.b:F3} A: {pixel.a:F3}";
				});
				schedule.Execute(() => {
					a.Update();
				}).Until(() => a.done);
			}

			var preview = previewContainer.Q("ImGUIPreview");

			preview.RegisterCallback<MouseMoveEvent>(e => {
				var localPos = GetPreviewMousePositionBetween01(e.mousePosition);
				node.mousePosition = localPos;
				UpdateColorPickerValues();

				if (e.imguiEvent.button == 1)
				{
					NotifyNodeChanged();
					node.lightPosition = (new Vector2(localPos.x, 1 - localPos.y) * 2 - Vector2.one) * node.tiling;
				}
			});

			// TODO: add source mip slider

			UpdateViewData();
			UpdateColorPickerValues();
		}

		protected override void DrawPreviewSettings(Texture texture)
		{
			if (node.material == null)
				return;

			// Try to get the input texture from material:
			var inputTexture = node.material.GetTextureWithDimension("_Source", node.settings.GetResolvedTextureDimension(owner.graph));

			EditorGUI.BeginChangeCheck();
			base.DrawPreviewSettings(inputTexture ?? texture);
			if (EditorGUI.EndChangeCheck())
				NotifyNodeChanged();
		}
	}
}