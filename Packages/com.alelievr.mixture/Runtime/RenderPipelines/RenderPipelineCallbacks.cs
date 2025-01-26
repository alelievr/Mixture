using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

#if UNITY_EDITOR
[UnityEditor.InitializeOnLoad]
#endif

public class RenderPipelineCallbacks
{
    static RenderPipelineCallbacks()
    {
        RenderPipelineManager.endContextRendering -= EndContextRendering;
        RenderPipelineManager.endContextRendering += EndContextRendering;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
    static void OnAfterAssembliesLoaded()
    {
        RenderPipelineManager.endContextRendering -= EndContextRendering;
        RenderPipelineManager.endContextRendering += EndContextRendering;
    }

    static void EndContextRendering(ScriptableRenderContext context, List<Camera> cameras)
    {
        foreach (var camera in cameras)
        {
            if (camera == null)
                continue;

            foreach (var g in MixturePass.runningGraphs)
                g.InvokeCommandBufferExecuted();
            
        }
        MixturePass.ReleaseAllTemporaryRTs();
    }
}