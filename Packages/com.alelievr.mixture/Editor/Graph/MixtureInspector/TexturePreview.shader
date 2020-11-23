Shader "Hidden/MixtureInspectorPreview"
{
    Properties
    {
        _MainTex0_2D ("_MainTex 0", 2D) = "" {}
        _MainTex1_2D ("_MainTex 1", 2D) = "" {}
        _MainTex0_3D ("_MainTex 0", 3D) = "" {}
        _MainTex1_3D ("_MainTex 1", 3D) = "" {}
        _MainTex0_Cube ("_MainTex 0", Cube) = "" {}
        _MainTex1_Cube ("_MainTex 1", Cube) = "" {}
		_Size("_Size", Vector) = (512.0,512.0,1.0,1.0)
		_Channels ("_Channels", Vector) = (1.0,1.0,1.0,1.0)
		_PreviewMip("_PreviewMip", Float) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Overlay" }
        LOD 100

        Pass
        {
			Blend SrcAlpha OneMinusSrcAlpha
			Cull Off
			ZWrite Off
			ZTest LEqual


            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature CRT_2D CRT_3D CRT_CUBE

			#include "Packages/com.alelievr.mixture/Editor/Resources/MixturePreview.hlsl"
            #include "Packages/com.alelievr.mixture/Editor/Resources/MixtureSRGB.hlsl"

            float4 _TextureSize;
            float _ComparisonSlider;
            float _Zoom;
            float4 _Pan;
            float _YRatio;
            float _Exp;
            float _FilterMode;
            float _CompareMode;
            float _ComparisonEnabled;
            float _IsSRGB0;
            float _IsSRGB1;

            #define MERGE_NAME(x, y) x##y

            sampler s_trilinear_repeat_sampler;
            sampler s_linear_repeat_sampler;
            sampler s_point_repeat_sampler;

            // Local copy/paste because we're in an CGProgram -_________-
            float3 LatlongToDirectionCoordinate(float2 coord)
            {
                float theta = coord.y * 3.14159265;
                float phi = (coord.x * 2.f * 3.14159265 - 3.14159265*0.5f);

                float cosTheta = cos(theta);
                float sinTheta = sqrt(1.0 - min(1.0, cosTheta*cosTheta));
                float cosPhi = cos(phi);
                float sinPhi = sin(phi);

                float3 direction = float3(sinTheta*cosPhi, cosTheta, sinTheta*sinPhi);
                direction.xy *= -1.0;
                return direction;
            }

#if CRT_2D
			Texture2D _MainTex0_2D;
			Texture2D _MainTex1_2D;
            #define TEXTURE_TYPE Texture2D

            #define SAMPLE_LEVEL(tex, samp, uv, mip) MERGE_NAME(tex,_2D).SampleLevel(samp, uv, mip)
#elif CRT_3D
			Texture3D _MainTex0_3D;
			Texture3D _MainTex1_3D;
            #define TEXTURE_TYPE Texture3D

            #define SAMPLE_LEVEL(tex, samp, uv, mip) MERGE_NAME(tex,_3D).SampleLevel(samp, uv, mip)
#elif CRT_CUBE
			TextureCube _MainTex0_Cube;
			TextureCube _MainTex1_Cube;
            #define TEXTURE_TYPE TextureCube

            #define SAMPLE_LEVEL(tex, samp, uv, mip) MERGE_NAME(tex,_Cube).SampleLevel(samp, LatlongToDirectionCoordinate(uv.xy), mip)
#endif

            float4 ApplyComparison(float2 uv, float4 c0, float4 c1)
            {
                if (!_ComparisonEnabled)
                    return c0;

                switch (_CompareMode)
                {
                    default:
                    case 0: // Side By Side
                        return frac(uv.x) < _ComparisonSlider ? c0 : c1;
                    case 1: // Onion skin
                        return lerp(c0, c1, _ComparisonSlider);
                    case 2: // Difference
                        return abs(c0 - c1);
                }
            }

            float4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;
                uv += float2(-_Pan.x, _Pan.y - 1);
                uv *= rcp(_Zoom.xx);
                float4 color0, color1;

                switch (_FilterMode)
                {
                    default:
                    case 0: // Point
                        color0 = SAMPLE_LEVEL(_MainTex0, s_point_repeat_sampler, float3(uv, 0), floor(_PreviewMip)) * _Channels;
                        color1 = SAMPLE_LEVEL(_MainTex1, s_point_repeat_sampler, float3(uv, 0), floor(_PreviewMip)) * _Channels;
                        break;
                    case 1: // Bilinear
                        color0 = SAMPLE_LEVEL(_MainTex0, s_linear_repeat_sampler, float3(uv, 0), floor(_PreviewMip)) * _Channels;
                        color1 = SAMPLE_LEVEL(_MainTex1, s_linear_repeat_sampler, float3(uv, 0), floor(_PreviewMip)) * _Channels;
                        break;
                    case 2: // Trilinear
                        color0 = SAMPLE_LEVEL(_MainTex0, s_trilinear_repeat_sampler, float3(uv, 0), floor(_PreviewMip)) * _Channels;
                        color1 = SAMPLE_LEVEL(_MainTex1, s_trilinear_repeat_sampler, float3(uv, 0), floor(_PreviewMip)) * _Channels;
                        break;
                }

                // Apply gamma if needed
                if (_IsSRGB0)
                    color0.xyz = LinearToSRGB(color0.xyz);
                if (_IsSRGB1)
                    color1.xyz = LinearToSRGB(color1.xyz);

                // TODO: blend the two colors with comparison mode
                float4 color = ApplyComparison(uv, color0, color1);

                // Apply exposure:
                color.rgb = color.rgb * exp2(_Exp);

                return MakePreviewColor(i, _TextureSize.zw, color);
            }
            ENDHLSL
        }
    }
}
