using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using GraphProcessor;
using UnityEngine.Rendering;
using Unity.Collections;
using System;

namespace Mixture
{
	[NodeCustomEditor(typeof(OutputNode))]
	public class OutputNodeView : MixtureNodeView
	{
		VisualElement	shaderCreationUI;
		VisualElement	materialEditorUI;
		MaterialEditor	materialEditor;
		OutputNode		outputNode;
		MixtureGraph    graph;

		static readonly Vector2 nodeViewSize = new Vector2(330, 480);

		protected override bool hasPreview => true;

		public override void Enable()
		{
			base.Enable();

			outputNode = nodeTarget as OutputNode;
			graph = owner.graph as MixtureGraph;
			outputNode.onTempRenderTextureUpdated += UpdatePreviewImage;

			// Fix the size of the node
			var currentPos = GetPosition();
			SetPosition(new Rect(currentPos.x, currentPos.y, nodeViewSize.x, nodeViewSize.y));

			graph.onOutputTextureUpdated += UpdatePreviewImage;

			UpdatePreviewImage();
			controlsContainer.Add(previewContainer);

			controlsContainer.Add(new Button(SaveTexture) {
				text = "Save"
			});
		}

		void UpdatePreviewImage()
		{
			CreateTexturePreview(ref previewContainer, outputNode.tempRenderTexture, outputNode.currentSlice);
		}

		// Write the rendertexture value to the graph main texture asset
		void SaveTexture()
		{
			// Retrieve the texture from the GPU:
			var src = outputNode.tempRenderTexture;
			var request = AsyncGPUReadback.Request(src, 0, 0, src.width, 0, src.height, 0, src.volumeDepth, (r) => {
				WriteRequestResult(r, graph.outputTexture);
			});

			request.Update();

			request.WaitForCompletion();
		}

		void WriteRequestResult(AsyncGPUReadbackRequest request, Texture output)
		{
			if (request.hasError)
			{
				Debug.LogError("Can't readback the texture from GPU");
				return ;
			}

			void FetchSlice(int slice, Action< Color32[] > SetPixelsColor32, Action< Color[] > SetPixelsColor)
			{
				NativeArray< Color32 >    	colors32;
				NativeArray< Color >    	colors;

				var outputFormat = (OutputFormat)output.graphicsFormat;
				switch (outputFormat)
				{
					case OutputFormat.RGBA_Float:
					case OutputFormat.RGB_Float:
						colors = request.GetData< Color >(slice);
						SetPixelsColor(colors.ToArray());
						break;
					case OutputFormat.RGBA_LDR:
					case OutputFormat.RGB_LDR:
						colors32 = request.GetData< Color32 >(slice);
						SetPixelsColor32(colors32.ToArray());
						break;
					case OutputFormat.RGBA_Half: // For now we don't support half readback
					case OutputFormat.RGB_Half:
					default:
						Debug.LogError("Can't readback an image with format: " + outputFormat);
						break;
				}
			}

			switch (output)
			{
				case Texture2D t:
					FetchSlice(0, t.SetPixels32, t.SetPixels);
					t.Apply();
					break;
				case Texture2DArray t:
					for (int i = 0; i < outputNode.tempRenderTexture.volumeDepth; i++)
						FetchSlice(i, colors => t.SetPixels32(colors, i), colors => t.SetPixels(colors, i));
					t.Apply();
					break;
				case Texture3D t:
					for (int i = 0; i < outputNode.tempRenderTexture.volumeDepth; i++)
						FetchSlice(i, colors => t.SetPixels32(colors, i), colors => t.SetPixels(colors, i));
					t.Apply();
					break;
				default:
					Debug.LogError(output + " is not a supported type for saving");
					return ;
			}

			EditorGUIUtility.PingObject(output);
		}
	}
}