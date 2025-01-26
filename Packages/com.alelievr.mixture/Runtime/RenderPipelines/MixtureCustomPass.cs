using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;
using Mixture;
using System.Collections.Generic;
using System;
using UnityEngine.Experimental.Playables;

class MixturePass : CustomPass
{
    public RenderTexture graph;
    [SerializeField, HideInInspector] internal MixtureGraph graphReference;
    MixtureGraphProcessor processor;

    [NonSerialized]
    List<RenderTexture> temporaryRTs = new();

    internal static HashSet<MixtureGraph> runningGraphs = new();
    static HashSet<MixturePass> runningPasses = new();

    protected override void Setup(ScriptableRenderContext renderContext, CommandBuffer cmd)
    {
        runningGraphs.Add(graphReference);
        runningPasses.Add(this);
    }

    protected override void Execute(CustomPassContext ctx)
    {
        if (graph == null)
            return;
        
        if (graph != graphReference?.mainOutputTexture)
        {
            runningGraphs.Remove(graphReference);
            graphReference = MixtureDatabase.GetGraphFromTexture(graph);
            runningGraphs.Add(graphReference);
        }

        if (graphReference == null || graphReference.type != MixtureGraphType.Realtime)
            return;

        if (processor == null || processor.graph != graphReference)
        {
            processor?.Dispose();
            processor = MixtureGraphProcessor.GetOrCreate(graphReference);
        }

        // Bind all params from the context
        foreach (var p in graphReference.exposedParameters)
        {
            if (p is RenderPipelineTextureParameter r)
            {
                var t = (RenderPipelineTexture)r.value;
                var rt = GetTextureFromType(ctx, t);

                // HDRP always uses texture2DArray for XR support even when it's not enabled.
                // Because Mixture doesn't support texture2DArray yet, we need to convert it to a 2D texture
                if (rt.dimension == TextureDimension.Tex2DArray)
                {
                    // TODO: depth support
                    var tmp = RenderTexture.GetTemporary(rt.width, rt.height, rt.depth, rt.graphicsFormat);
                    ctx.cmd.Blit(rt, tmp, 0, 0); // TODO: XR support
                    rt = tmp;
                    temporaryRTs.Add(rt);
                }

                t.renderPipelineTexture = rt;
            }
        }

        processor.Run(ctx.cmd);

        // Copy the mixture output to the current custom pass output
        SetRenderTargetAuto(ctx.cmd);
        ctx.cmd.Blit(graphReference.mainOutputTexture, ctx.cameraColorBuffer, 0, 0); // No XR support yet
    }

    RenderTexture GetTextureFromType(CustomPassContext ctx, RenderPipelineTexture type)
    {
        switch (type.type)
        {
            case RenderPipelineTextureType.Color: return ctx.cameraColorBuffer;
            case RenderPipelineTextureType.Depth: return ctx.cameraDepthBuffer;
            case RenderPipelineTextureType.Normal: return ctx.cameraNormalBuffer;
            case RenderPipelineTextureType.Smoothness: return ctx.cameraNormalBuffer;
            case RenderPipelineTextureType.Motion: return ctx.cameraMotionVectorsBuffer;
            case RenderPipelineTextureType.RenderingLayers: return null; // TODO: support this!
            case RenderPipelineTextureType.IsSky: return ctx.cameraDepthBuffer;
            case RenderPipelineTextureType.Thickness: return null;
            case RenderPipelineTextureType.CustomColor: return ctx.customColorBuffer.Value;
            case RenderPipelineTextureType.CustomDepth: return ctx.customDepthBuffer.Value;
            default: return null;
        }
    }

    protected override void Cleanup()
    {
        processor?.Dispose();
        runningGraphs.Remove(graphReference);
        runningPasses.Remove(this);
    }

    internal static void ReleaseAllTemporaryRTs()
    {
        foreach (var p in runningPasses)
            p.temporaryRTs.ForEach(rt => RenderTexture.ReleaseTemporary(rt));
    }
}