Shader "Hidden/Mixture/LineIntegralConvolution"
{
    Properties
    {
        // By default a shader node is supposed to handle all the input texture dimension, we use a prefix to determine which one is used
        [InlineTexture]_TFM_2D("TFM", 2D) = "white" {}
        [InlineTexture]_Noise_2D("Noise", 2D) = "white" {}
        _KernelRadius("Kernel Radius", Range(1, 10)) = 5
        // Other parameters
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
        }
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #include "Packages/com.alelievr.mixture/Runtime/Shaders/MixtureFixed.hlsl"
            #pragma vertex CustomRenderTextureVertexShader
            #pragma fragment MixtureFragment
            #pragma target 3.0

            // The list of defines that will be active when processing the node with a certain dimension
            #pragma shader_feature CRT_2D CRT_3D CRT_CUBE

            // This macro will declare a version for each dimention (2D, 3D and Cube)
            TEXTURE_SAMPLER_X(_TFM);
            TEXTURE_SAMPLER_X(_Noise);
            
            StructuredBuffer<float> _Kernel;
            SamplerState point_clamp_sampler;
            SamplerState linear_clamp_sampler;
            int _KernelRadius;
            sampler2D d;


            

            float4 mixture(v2f_customrendertexture i) : SV_Target
            {
                // The SAMPLE_X macro handles sampling for 2D, 3D and cube textures
                return SAMPLE_X(_TFM, i.localTexcoord.xyz, i.direction);
            }
            ENDHLSL
        }
    }
}