Shader "Hidden/Mixture/CompareTextures"
{
    Properties
    {
        // By default a shader node is supposed to handle all the input texture dimension, we use a prefix to determine which one is used
        [InlineTexture]_A_2D("Source", 2D) = "white" {}
        [InlineTexture]_A_3D("Source", 3D) = "white" {}
        [InlineTexture]_A_Cube("Source", Cube) = "white" {}

        [InlineTexture]_B_2D("Source", 2D) = "white" {}
        [InlineTexture]_B_3D("Source", 3D) = "white" {}
        [InlineTexture]_B_Cube("Source", Cube) = "white" {}

        [Enum(Horizontal, 0, Vertical, 1)] _Axis("Rotation Mode", Float) = 0
        [RangeDrawer]_Threshold("Threshold", Range(0, 1)) = 0.5
        [Toggle]_Invert("Invert", Float) = 0
        


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
            TEXTURE_SAMPLER_X(_A);
            TEXTURE_SAMPLER_X(_B);
            float _Threshold;
            float _Axis;
            float _Invert;

            float4 mixture(v2f_customrendertexture i) : SV_Target
            {
                // The SAMPLE_X macro handles sampling for 2D, 3D and cube textures
                float4 colorA = SAMPLE_X(_A, i.localTexcoord.xyz, i.direction);
                float4 colorB = SAMPLE_X(_B, i.localTexcoord.xyz, i.direction);
                bool invert = _Invert > 0.5;
                switch (_Axis)
                {
                case 0:
                    if(i.localTexcoord.x < _Threshold)
                        return invert ? colorA : colorB;
                    else
                        return invert ? colorB : colorA;
                    break;
                case 1:
                    if(i.localTexcoord.y < _Threshold)
                        return invert ? colorA : colorB;
                    else
                        return invert ? colorB : colorA;
                    break;
                }
                return 0;
            }
            ENDHLSL
        }
    }
}