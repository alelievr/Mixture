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
        SceneNode.OutputMode    mode;
        Camera                  targetCamera;

        internal void SetOutputSettings(SceneNode.OutputMode mode, Camera targetCamera)
        {
            this.mode = mode;
            this.targetCamera = targetCamera;
        }

        protected override void Setup(ScriptableRenderContext renderContext, CommandBuffer cmd)
        {
            outputBufferMaterial = CoreUtils.CreateEngineMaterial(Shader.Find("Hidden/Mixture/OutputBuffer"));
            properties = new MaterialPropertyBlock();
        }

        protected override void Execute(ScriptableRenderContext renderContext, CommandBuffer cmd, HDCamera hdCamera, CullingResults cullingResult)
        {
            if (hdCamera.camera != targetCamera)
                return;

            // For color we don't need to do anything
            if (mode != SceneNode.OutputMode.Color)
            {
                properties.SetFloat("_OutputMode", (int)mode);
                GetCameraBuffers(out var color, out var depth);
                CoreUtils.SetRenderTarget(cmd, color, ClearFlag.Color, Color.clear);
                CoreUtils.DrawFullScreen(cmd, outputBufferMaterial, properties, shaderPassId: 0);
            }
        }

        protected override void Cleanup()
        {
            CoreUtils.Destroy(outputBufferMaterial);
        }
    }
}
#endif