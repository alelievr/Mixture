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
			HLSLPROGRAM
			#include "Packages/com.alelievr.mixture/Runtime/Shaders/MixtureFixed.hlsl"
            #include "Packages/com.alelievr.mixture/Editor/Resources/HistogramData.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #pragma vertex CustomRenderTextureVertexShader
			#pragma fragment MixtureFragment
			#pragma target 5.0

			// The list of defines that will be active when processing the node with a certain dimension
            #pragma shader_feature CRT_2D CRT_3D CRT_CUBE
            // #pragma enable_d3d11_debug_symbols

            TEXTURE_X(_Input);

            Texture2D<float> _InterpolationCurve;
            Texture2D<float> _InterpolationCurveR;
            Texture2D<float> _InterpolationCurveG;
            Texture2D<float> _InterpolationCurveB;
            float3 _RcpTextureSize;

            StructuredBuffer<LuminanceData> _Luminance;

            float _Mode;
            float _ChannelMode;
            float _ManualMin;
            float _ManualMax;

            void GetLuminanceRemapValues(out float minLuminace, out float maxLuminance)
            {
                if (_Mode == 0) // Manual
                {
                    minLuminace = _ManualMin;
                    maxLuminance = _ManualMax;
                }
                else // Authomatic
                {
                    minLuminace = _Luminance[0].minLuminance;
                    maxLuminance = _Luminance[0].maxLuminance;
                }
            }

            void GetTextureAbsoluteLuminanceValues(out float minLuminace, out float maxLuminance)
            {
                minLuminace = _Luminance[0].minLuminance;
                maxLuminance = _Luminance[0].maxLuminance;
            }

			float4 mixture (v2f_customrendertexture i) : SV_Target
			{
                // TODO: function to turn the id into direction / uv for cube / 3D
                float3 uv = GetDefaultUVs(i);
                uv += _RcpTextureSize * 0.5;
                float4 input = SAMPLE_X_NEAREST_CLAMP(_Input, uv, uv);

                float minRemapLum, maxRemapLum;
                GetLuminanceRemapValues(minRemapLum, maxRemapLum);

                float minAbsoluteLum, maxAbsoluteLum;
                GetTextureAbsoluteLuminanceValues(minAbsoluteLum, maxAbsoluteLum);

                float totalMinLum = (minRemapLum - minAbsoluteLum) / (maxRemapLum - minRemapLum);
                float totalMaxLum = (maxRemapLum) / (maxRemapLum - minRemapLum);

                input.rgb -= minRemapLum;
                input.rgb /= (maxRemapLum - minRemapLum);

                // TODO: use the channel mode to separate the remap of 3 channels

                if (_ChannelMode == 0) // RGB remap
                {
                    // remap luminance between 0 and 1 to sample the curve:
                    float clampedLuminance = clamp(Luminance(input.rgb), totalMinLum, totalMaxLum);
                    float luminance01 = (clampedLuminance - totalMinLum) * rcp(totalMaxLum - totalMinLum);

                    // Remap luminance with curve
                    float t = luminance01;
                    luminance01 = _InterpolationCurve.SampleLevel(s_linear_clamp_sampler, luminance01, 0).r;

                    // Remap luminance between min and max
                    float correctedLuminance = luminance01 * (totalMaxLum - totalMinLum) + totalMinLum;
                    // Correct the color with the new luminance
                    float luminanceOffset = correctedLuminance - clampedLuminance;

                    input.rgb *= 1 + luminanceOffset;
                }
                else // Per channel remap curves
                {
                    input.r = _InterpolationCurveR.SampleLevel(s_linear_clamp_sampler, input.r, 0);
                    input.g = _InterpolationCurveG.SampleLevel(s_linear_clamp_sampler, input.g, 0);
                    input.b = _InterpolationCurveB.SampleLevel(s_linear_clamp_sampler, input.b, 0);
                }

                return input;
			}
			ENDHLSL
		}
	}
}