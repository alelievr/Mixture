using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using System.Collections.Generic;
using Mixture;
using System;
using System.Reflection;

public class MixtureFeature : ScriptableRendererFeature
{
    [Serializable]
    public class MixtureFeatureSettings
    {
        public RenderTexture graph;
        [SerializeField, HideInInspector] internal MixtureGraph graphReference;
    }

    public MixtureFeatureSettings settings = new MixtureFeatureSettings();
    
    internal static HashSet<MixtureFeature> runningFeatures = new();
    internal static HashSet<MixtureGraph> runningGraphs = new();

    class CustomRenderPass : ScriptableRenderPass
    {
        MixtureFeatureSettings settings;
        MixtureGraphProcessor processor;

        public CustomRenderPass(MixtureFeatureSettings settings)
        {
            this.settings = settings;
        }

        // This class stores the data needed by the RenderGraph pass.
        // It is passed as a parameter to the delegate function that executes the RenderGraph pass.
        class PassData
        {
            public MixtureGraph graph;
            public MixtureGraphProcessor processor;
            public UniversalResourceData universalResourceData;
            public TextureHandle outputTarget;
            public TextureHandle colorTexture;
            public TextureHandle depthTexture;
            public TextureHandle normalTexture;
            public TextureHandle motionTexture;
            public TextureHandle renderingLayersTexture;
            public TextureHandle ssaoTexture;
        }

        static FieldInfo wrappedCmd = typeof(UnsafeCommandBuffer).GetField("m_WrappedCommandBuffer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // This static method is passed as the RenderFunc delegate to the RenderGraph render pass.
        // It is used to execute draw commands.
        static void ExecutePass(PassData data, UnsafeGraphContext context)
        {
            // Bind all params from the context
            foreach (var p in data.graph.exposedParameters)
            {
                if (p is RenderPipelineTextureParameter r)
                {
                    var t = (RenderPipelineTexture)r.value;
                    RenderTexture rt = GetRenderTextureFromType(data, t);

                    t.renderPipelineTexture = rt;
                }
            }

            RenderTexture outputRT = data.outputTarget;
            data.graph.settings.width = outputRT.width;
            data.graph.settings.height = outputRT.height;

            CommandBuffer cmd = wrappedCmd.GetValue(context.cmd) as CommandBuffer;
            data.processor.Run(cmd);

            // Copy the mixture output to the current custom pass output
            cmd.Blit(data.graph.mainOutputTexture, data.outputTarget, 0, 0); // No XR support yet
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            if (settings.graph == null)
                return;

            if (processor == null || processor.graph != settings.graphReference)
            {
                processor?.Dispose();
                processor = MixtureGraphProcessor.GetOrCreate(settings.graphReference);
            }

            // This adds a raster render pass to the graph, specifying the name and the data type that will be passed to the ExecutePass function.
            using (var builder = renderGraph.AddUnsafePass<PassData>("Mixture Pass", out var passData))
            {
                UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

                passData.graph = settings.graphReference;
                passData.processor = processor;
                // TODO: a UI to control this
                passData.outputTarget = resourceData.cameraColor;
                builder.UseTexture(resourceData.activeColorTexture, AccessFlags.Write);

                passData.universalResourceData = resourceData;
                passData.colorTexture = resourceData.cameraColor;
                passData.depthTexture = resourceData.cameraDepth;
                passData.normalTexture = resourceData.cameraNormalsTexture;
                passData.motionTexture = resourceData.motionVectorColor;
                passData.renderingLayersTexture = resourceData.renderingLayersTexture;
                passData.ssaoTexture = resourceData.ssaoTexture;

                // Read all the resources required by the graph that comes from the pipeline
                foreach (var p in settings.graphReference.exposedParameters)
                {
                    if (p is RenderPipelineTextureParameter r)
                    {
                        var rt = GetTextureFromType(resourceData, (RenderPipelineTexture)r.value);
                        if (rt.IsValid())
                            builder.UseTexture(rt, AccessFlags.Read);
                    }
                }

                // Assigns the ExecutePass function to the render pass delegate. This will be called by the render graph when executing the pass.
                builder.SetRenderFunc((PassData data, UnsafeGraphContext context) => ExecutePass(data, context));
            }
        }

        static TextureHandle GetTextureFromType(UniversalResourceData res, RenderPipelineTexture t)
        {
            switch (t.type)
            {
                case RenderPipelineTextureType.Color: return res.cameraColor;
                case RenderPipelineTextureType.Depth: return res.cameraDepth;
                case RenderPipelineTextureType.Normal: return res.cameraNormalsTexture;
                case RenderPipelineTextureType.Motion: return res.motionVectorColor;
                case RenderPipelineTextureType.RenderingLayers: return res.renderingLayersTexture;
                case RenderPipelineTextureType.SSAO: return res.ssaoTexture;
                default: return TextureHandle.nullHandle;
            }
        }

        static TextureHandle GetRenderTextureFromType(PassData data, RenderPipelineTexture t)
        {
            switch (t.type)
            {
                case RenderPipelineTextureType.Color: return data.colorTexture;
                case RenderPipelineTextureType.Depth: return data.depthTexture;
                case RenderPipelineTextureType.Normal: return data.normalTexture;
                case RenderPipelineTextureType.Motion: return data.motionTexture;
                case RenderPipelineTextureType.RenderingLayers: return data.renderingLayersTexture;
                case RenderPipelineTextureType.SSAO: return data.ssaoTexture;
                default: return TextureHandle.nullHandle;
            }
        }

        // NOTE: This method is part of the compatibility rendering path, please use the Render Graph API above instead.
        // Cleanup any allocated resources that were created during the execution of this render pass.
        public override void OnCameraCleanup(CommandBuffer cmd)
        {
        }
    }

    CustomRenderPass pass;

    /// <inheritdoc/>
    public override void Create()
    {
        pass = new CustomRenderPass(settings);

        // Configures where the render pass should be injected.
        pass.renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
        runningFeatures.Add(this);
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        // XR not supported for now
        if (renderingData.cameraData.xrRendering)
            return;

        if (settings.graph != settings.graphReference?.mainOutputTexture)
        {
            runningGraphs.Remove(settings.graphReference);
            settings.graphReference = MixtureDatabase.GetGraphFromTexture(settings.graph);
            runningGraphs.Add(settings.graphReference);
        }

        if (settings.graphReference == null || settings.graphReference.type != MixtureGraphType.Realtime)
            return;

        renderer.EnqueuePass(pass);
    }

    protected override void Dispose(bool disposing)
    {
        runningGraphs.Remove(settings.graphReference);
        runningFeatures.Remove(this);
        base.Dispose(disposing);
    }
}
