using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using GraphProcessor;
using System.Collections.Generic;
using UnityEngine.Rendering;
using Unity.Collections;
using System;
using TextureCompressionQuality = UnityEngine.TextureCompressionQuality;
using UnityEngine.Experimental.Rendering;

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
			style.width = nodeViewSize.x;
			style.height = nodeViewSize.y;

			graph.onOutputTextureUpdated += UpdatePreviewImage;

			UpdatePreviewImage();
			controlsContainer.Add(previewContainer);

			AddCompressionSettings();

			controlsContainer.Add(new Button(SaveTexture) {
				text = "Save"
			});
		}
		
		void AddCompressionSettings()
		{
			var formatField = new EnumField("Format", outputNode.compressionFormat);
			formatField.RegisterValueChangedCallback((e) => outputNode.compressionFormat = (TextureFormat)e.newValue);
			var qualityField = new EnumField("Quality", outputNode.compressionQuality);
			qualityField.RegisterValueChangedCallback((e) => outputNode.compressionQuality = (TextureCompressionQuality)e.newValue);

			if (!outputNode.enableCompression)
			{
				qualityField.ToggleInClassList("Hidden");
				formatField.ToggleInClassList("Hidden");
			}
			
			var enabledField = new Toggle("Compression") { value = outputNode.enableCompression };
			enabledField.RegisterValueChangedCallback((e) => {
				qualityField.ToggleInClassList("Hidden");
				formatField.ToggleInClassList("Hidden");
				outputNode.enableCompression = e.newValue;
			});

			controlsContainer.Add(enabledField);
			controlsContainer.Add(formatField);
			controlsContainer.Add(qualityField);
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
					if (outputNode.enableCompression)
						t = CompressTexture(t);
					break;
				case Texture2DArray t:
					for (int i = 0; i < outputNode.tempRenderTexture.volumeDepth; i++)
						FetchSlice(i, colors => t.SetPixels32(colors, i), colors => t.SetPixels(colors, i));
					t.Apply();
					break;
				case Texture3D t:
					List< Color32 >	colors32List = new List< Color32 >();
					List< Color >	colorsList = new List< Color >();
					for (int i = 0; i < outputNode.tempRenderTexture.volumeDepth; i++)
						FetchSlice(0, c => colors32List.AddRange(c), c => colorsList.AddRange(c));
					
					if (colors32List.Count != 0)
						t.SetPixels32(colors32List.ToArray());
					else
						t.SetPixels(colorsList.ToArray());

					t.Apply();
					break;
				default:
					Debug.LogError(output + " is not a supported type for saving");
					return ;
			}

			EditorGUIUtility.PingObject(output);
		}

		Texture2D CompressTexture(Texture2D t)
		{
			Texture2D compressedTexture = new Texture2D(t.width, t.height, t.graphicsFormat, TextureCreationFlags.None);
			compressedTexture.SetPixels(t.GetPixels());
			compressedTexture.Apply();
			EditorUtility.CompressTexture(compressedTexture, outputNode.compressionFormat, (UnityEditor.TextureCompressionQuality)outputNode.compressionQuality);
			return compressedTexture;
		}
	}
}