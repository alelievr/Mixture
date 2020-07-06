Shader "Hidden/MixtureInspectorPreview"
{
    Properties
    {
        _MainTex0 ("_MainTex 0", 2D) = "" {}
        _MainTex1 ("_MainTex 1", 2D) = "" {}
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

			#include "Packages/com.alelievr.mixture/Editor/Resources/MixturePreview.hlsl"

            sampler s_trilinear_repeat_sampler;
            sampler s_linear_repeat_sampler;
            sampler s_point_repeat_sampler;

			Texture2D _MainTex0;
			float4 _MainTex0_ST;
			float4 _MainTex0_TexelSize;
			Texture2D _MainTex1;
			float4 _MainTex1_ST;
			float4 _MainTex1_TexelSize;

            float4 _Size;
            float _ComparisonMode;
            float _ComparisonSlider;
            float _Zoom;
            float4 _Pan;
            float _YRatio;
            float _Exp;
            float _FilterMode;

            float4 SampleColor(Texture2D tex, float2 uv, float mip)
            {
                switch (_FilterMode)
                {
                    default:
                    case 0: // Point
                        return tex.SampleLevel(s_point_repeat_sampler, uv, mip);
                    case 1: // Bilinear
                        return tex.SampleLevel(s_linear_repeat_sampler, uv, mip);
                    case 2: // Trilinear
                        return tex.SampleLevel(s_trilinear_repeat_sampler, uv, mip);
                }
            }

            float4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;
                uv += float2(-_Pan.x, _Pan.y - 1);
                uv *= rcp(_Zoom.xx);

				float4 color0 = SampleColor(_MainTex0, uv, floor(_PreviewMip)) * _Channels;
				float4 color1 = SampleColor(_MainTex1, uv, floor(_PreviewMip)) * _Channels;

                // TODO: blend the two colors with comparison mode
                float4 color = frac(uv.x) < _ComparisonSlider ? color0 : color1;

                // Apply exposure:
                color.rgb = color.rgb * exp2(_Exp);

                return MakePreviewColor(i, _MainTex0_TexelSize.zw, color);
            }
            ENDHLSL
        }
    }
}
