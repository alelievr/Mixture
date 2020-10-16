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

		[RuntimeInitializeOnLoadMethod]
		static void Load()
		{
			// TODO: custom CRT sorting in the  CRT manager to manage the case where we have multiple runtime CRTs on the same branch and they need to be updated
			// in the correct order even if their materials/textures are not referenced into their params (they can be connected to a compute shader node for example).
			CustomTextureManager.onBeforeCustomTextureUpdated -= BeforeCustomRenderTextureUpdate;
			CustomTextureManager.onBeforeCustomTextureUpdated += BeforeCustomRenderTextureUpdate;
		}

		static void BeforeCustomRenderTextureUpdate(CommandBuffer cmd, CustomRenderTexture crt)
		{
            MixtureGraph graph = null;

// Currently we can only check that a CRT is linked to a graph in the editor and not in a player.
// This limitation will be fixed with the assetbundle system (hopefully).
#if UNITY_EDITOR
			string graphPath = UnityEditor.AssetDatabase.GetAssetPath(crt);

            if (!String.IsNullOrEmpty(graphPath))
            {
                graph = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(graphPath).OfType<MixtureGraph>().FirstOrDefault();
            }
#else
			graph = MixtureDatabase.GetGraphFromTexture(crt);
#endif

			// If the graph is valid and realtime
			if (graph != null && graph.isRealtime == true)
			{
				MixtureGraphProcessor.processorInstances.TryGetValue(graph, out var processorSet);
				if (processorSet == null)
				{
					var processor = new MixtureGraphProcessor(graph);
					// Relay the event to the processor
					processor.BeforeCustomRenderTextureUpdate(cmd, crt);
				}
				else
				{
					foreach (var processor in processorSet)
					{
						// Relay the event to the processor
                        if (processor.isProcessing == 0)
                            processor.BeforeCustomRenderTextureUpdate(cmd, crt);
					}
				}
			}
		}
	}
}
