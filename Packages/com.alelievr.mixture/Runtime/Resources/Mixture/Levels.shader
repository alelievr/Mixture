Shader "Hidden/Mixture/Levels"
{
    Properties
	{
		[InlineTexture]_Input_2D("Input", 2D) = "black" {}
		[InlineTexture]_Input_3D("Input", 3D) = "black" {}
		[InlineTexture]_Input_Cube("Input", Cube) = "black" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#include "Packages/com.alelievr.mixture/Runtime/Shaders/MixtureFixed.cginc"
            #include "Packages/com.alelievr.mixture/Editor/Resources/HistogramData.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #pragma vertex CustomRenderTextureVertexShader
			#pragma fragment MixtureFragment
			#pragma target 5.0

			// The list of defines that will be active when processing the node with a certain dimension
            #pragma shader_feature CRT_2D CRT_3D CRT_CUBE
            #pragma enable_d3d11_debug_symbols

            TEXTURE_X(_Input);

            Texture2D<float> _InterpolationCurve;
            float3 _RcpTextureSize;

            StructuredBuffer<LuminanceData> _Luminance;

			float4 mixture (v2f_customrendertexture i) : SV_Target
			{
                // TODO: function to turn the id into direction / uv for cube / 3D
                float3 uv = GetDefaultUVs(i);
                uv += _RcpTextureSize * 0.5;
                float4 input = SAMPLE_X_NEAREST_CLAMP(_Input, uv, uv);

                float minLum = _Luminance[0].minLuminance;
                float maxLum = _Luminance[0].maxLuminance;

                // return float4(minLum, (maxLum - 0.45) * 50, 0, 1);

                input.rgb -= minLum;
                input.rgb /= (maxLum - minLum);

                // remap luminance between 0 and 1 to sample the curve:
                float clampedLuminance = clamp(Luminance(input.rgb), minLum, maxLum);
                float luminance01 = (clampedLuminance - minLum) * rcp(maxLum - minLum);

                // Remap luminance with curve
                float t = luminance01;
                luminance01 = _InterpolationCurve.SampleLevel(s_linear_clamp_sampler, luminance01, 0).r;

                // Remap luminance between min and max
                float correctedLuminance = luminance01 * (maxLum - minLum) + minLum;
                // Correct the color with the new luminance
                float luminanceOffset = correctedLuminance - clampedLuminance;
                // float3 D65 = float3(0.2126729, 0.7151522, 0.0721750);

                input.rgb *= 1 + luminanceOffset;

                float4 color = luminanceOffset; //saturate(1 - abs(3 * luminance01 / 3 - float4(0, 1, 2, 3)));

                return input;
			}
			ENDCG
		}
	}
}