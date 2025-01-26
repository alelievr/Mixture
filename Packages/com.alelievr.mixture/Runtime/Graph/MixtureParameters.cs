using UnityEngine;
using GraphProcessor;
using System;

namespace Mixture
{
    [Serializable]
	class TextureParameter : ExposedParameter
    {
        [SerializeField] Texture val;

        public override object value { get => val; set => val = (Texture)value; }
        public override Type GetValueType() => typeof(Texture);
    }

    [Serializable]
	class Texture3DParameter : ExposedParameter
    {
        [SerializeField] Texture3D val;

        public override object value { get => val; set => val = (Texture3D)value; }
        public override Type GetValueType() => typeof(Texture3D);
    }

    [Serializable]
	class CubemapParameter : ExposedParameter
    {
        [SerializeField] Cubemap val;

        public override object value { get => val; set => val = (Cubemap)value; }
        public override Type GetValueType() => typeof(Cubemap);
    }

    [Serializable]
	class ComputeBufferParameter : ExposedParameter
    {
        [SerializeField] ComputeBuffer val;

        public override object value { get => val; set => val = (ComputeBuffer)value; }
        public override Type GetValueType() => typeof(ComputeBuffer);
    }

    public enum RenderPipelineTextureType
    {
        Color,
        Depth,
        Normal,
        Smoothness,
        Motion,
        RenderingLayers,
        IsSky,
        Thickness,
        CustomColor,
        CustomDepth,
        SSAO,
    }

    [Serializable]
    public class RenderPipelineTexture
    {
        public RenderPipelineTextureType type;
        [NonSerialized] internal RenderTexture renderPipelineTexture;
    }

    [Serializable]
	class RenderPipelineTextureParameter : ExposedParameter
    {
        [SerializeField] RenderPipelineTexture val;

        public override object value { get => val; set => val = (RenderPipelineTexture)value; }
        public override Type GetValueType() => typeof(RenderPipelineTexture);
    }
}
