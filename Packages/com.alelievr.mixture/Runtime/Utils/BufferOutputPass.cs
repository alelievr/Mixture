#if MIXTURE_HDRP
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;

namespace Mixture
{
    [HideInInspector]
    class BufferOutputPass : CustomPass
    {
        Material                outputBufferMaterial;
        MaterialPropertyBlock   properties;
        PrefabCaptureNode.OutputMode    mode;
        Camera                  targetCamera;

        protected override bool executeInSceneView => false;

        internal void SetOutputSettings(PrefabCaptureNode.OutputMode mode, Camera targetCamera)
        {
            this.mode = mode;
            this.targetCamera = targetCamera;
        }

        protected override void Setup(ScriptableRenderContext renderContext, CommandBuffer cmd)
        {
            outputBufferMaterial = CoreUtils.CreateEngineMaterial(Shader.Find("Hidden/Mixture/OutputBuffer"));
            properties = new MaterialPropertyBlock();
        }

        protected override void Execute(CustomPassContext ctx)
        {
            if (ctx.hdCamera.camera != targetCamera)
                return;

            // For color we don't need to do anything
            if (mode != PrefabCaptureNode.OutputMode.Color)
            {
                properties.SetTexture("_NormalBufferTexture", ctx.cameraNormalBuffer);
                properties.SetFloat("_OutputMode", (int)mode);
                CoreUtils.SetRenderTarget(ctx.cmd, ctx.cameraColorBuffer, ClearFlag.Color, Color.clear);
                CoreUtils.DrawFullScreen(ctx.cmd, outputBufferMaterial, properties, shaderPassId: 0);
            }
        }

        protected override void Cleanup()
        {
            CoreUtils.Destroy(outputBufferMaterial);
        }
    }
}
#endif