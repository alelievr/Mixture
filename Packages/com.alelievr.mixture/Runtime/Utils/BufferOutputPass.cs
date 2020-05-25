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
        int i;

        internal void SetOutputMode(SceneNode.OutputMode mode)
        {
            this.mode = mode;
            i = (int)mode;
        }

        protected override void Setup(ScriptableRenderContext renderContext, CommandBuffer cmd)
        {
            outputBufferMaterial = CoreUtils.CreateEngineMaterial(Shader.Find("Hidden/Mixture/OutputBuffer"));
            properties = new MaterialPropertyBlock();
            i = 42;
        }

        protected override void Execute(ScriptableRenderContext renderContext, CommandBuffer cmd, HDCamera hdCamera, CullingResults cullingResult)
        {
            if (hdCamera.camera.cameraType == CameraType.SceneView)
                return;

            // For color we don't need to do anything
            if (mode != SceneNode.OutputMode.Color)
            {
                properties.SetFloat("_OutputMode", (int)mode);
                GetCameraBuffers(out var color, out var depth);
                CoreUtils.SetRenderTarget(cmd, color, ClearFlag.Color);
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