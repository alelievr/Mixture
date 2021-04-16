using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using GraphProcessor;
using UnityEngine.Rendering;
using System;

namespace Mixture
{
#if UNITY_EDITOR
	[UnityEditor.InitializeOnLoad]
#endif
	public class MixtureProcessingCallbacks
	{
		static MixtureProcessingCallbacks() => Load();
		static List<CustomRenderTexture> realtimeMixtureTextures = new List<CustomRenderTexture>();

		[RuntimeInitializeOnLoadMethod]
		static void Load()
		{
			RenderPipelineManager.beginFrameRendering -= UpdateRealtimeMixtures;
			RenderPipelineManager.beginFrameRendering += UpdateRealtimeMixtures;
			CustomRenderTextureManager.textureLoaded -= OnCRTLoaded;
			CustomRenderTextureManager.textureLoaded += OnCRTLoaded;
			CustomRenderTextureManager.textureUnloaded -= OnCRTUnloaded;
			CustomRenderTextureManager.textureUnloaded += OnCRTUnloaded;

			// TODO: custom CRT sorting in the  CRT manager to manage the case where we have multiple runtime CRTs on the same branch and they need to be updated
			// in the correct order even if their materials/textures are not referenced into their params (they can be connected to a compute shader node for example).
			// CustomTextureManager.onBeforeCustomTextureUpdated -= BeforeCustomRenderTextureUpdate;
			// CustomTextureManager.onBeforeCustomTextureUpdated += BeforeCustomRenderTextureUpdate;
		}
		
		static void UpdateRealtimeMixtures(ScriptableRenderContext ctx, Camera[] cameras)
		{

		}

		static void OnCRTLoaded(CustomRenderTexture crt)
		{
			var graph = MixtureDatabase.GetGraphFromTexture(crt);

			if (graph != null)
				realtimeMixtureTextures.Add(crt);
			Debug.Log("Load: " + crt);
		}

		static void OnCRTUnloaded(CustomRenderTexture crt)
		{
			realtimeMixtureTextures.Remove(crt);
			Debug.Log("Unload: " + crt);
		}

		// Now the only CRT that can be called here are graph outputs CRTs
		// TODO: check how to handle multi-output realtime crts
		static void BeforeCustomRenderTextureUpdate(CustomRenderTexture crt)
		{
            MixtureGraph graph = MixtureDatabase.GetGraphFromTexture(crt);;

			// If the graph is valid and realtime
			if (graph != null && graph.type == MixtureGraphType.Realtime)
			{
				MixtureGraphProcessor.processorInstances.TryGetValue(graph, out var processorSet);
				if (processorSet == null)
				{
					var processor = new MixtureGraphProcessor(graph);
					// Relay the event to the processor
					processor.Run();
				}
				else
				{
					foreach (var processor in processorSet)
					{
						// Relay the event to the processor
                        if (processor.isProcessing == 0)
                            processor.Run();
					}
				}
			}
		}
	}
}
