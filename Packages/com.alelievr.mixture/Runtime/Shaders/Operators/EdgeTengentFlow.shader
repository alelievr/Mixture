Shader "Hidden/Mixture/EdgeTengentFlow"
{
    Properties
    {
        // By default a shader node is supposed to handle all the input texture dimension, we use a prefix to determine which one is used
        [InlineTexture]_StructuredTensor_2D("Source", 2D) = "white" {}
        [InlineTexture]_StructuredTensor_3D("Source", 3D) = "white" {}
        [InlineTexture]_StructuredTensor_Cube("Source", Cube) = "white" {}

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
            TEXTURE_SAMPLER_X(_StructuredTensor);

            float4 mixture(v2f_customrendertexture i) : SV_Target
            {
                // The SAMPLE_X macro handles sampling for 2D, 3D and cube textures
                float3 tensor = SAMPLE_X(_StructuredTensor, i.localTexcoord.xyz, i.direction);

                half E = tensor.x;
                half G = tensor.y;
                half F = tensor.z;
                half D = sqrt((E - G) * (E - G) + 4.0h * F * F);

                half L1 = 0.5h * (E + G + D);
                half L2 = 0.5h * (E + G - D);

                half phi = length(half2(-F, L1 - E)) > 0.0h ? atan2(-F, L1 - E) : atan2(0.0h, 1.0h);
                half A = (L1 + L2 > 0.0h) ? (L1 - L2) / (L1 + L2) : 0.0h;

                return float4(cos(phi), sin(phi), phi, A);
            }
            ENDHLSL
        }
    }
}