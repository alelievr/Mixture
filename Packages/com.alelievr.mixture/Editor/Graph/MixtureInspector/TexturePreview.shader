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


            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

			#include "Packages/com.alelievr.mixture/Editor/Resources/MixturePreview.hlsl"

			sampler2D _MainTex0;
			float4 _MainTex0_ST;
			float4 _MainTex0_TexelSize;
			sampler2D _MainTex1;
			float4 _MainTex1_ST;
			float4 _MainTex1_TexelSize;

            float4 _Size;
            float _ComparisonMode;
            float _ComparisonSlider;
            float _Zoom;
            float4 _Pan;

            float4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;// + _Pan.xy;
                uv += float2(-_Pan.x, _Pan.y - 1);
                uv *= rcp(_Zoom.xx);


                // return float4(abs(uv), 0, 1);

				float4 color0 = tex2Dlod(_MainTex0, float4(uv, 0.0, floor(_PreviewMip))) * _Channels;
				float4 color1 = tex2Dlod(_MainTex1, float4(uv, 0.0, floor(_PreviewMip))) * _Channels;

                // TODO: blend the two colors with comparison mode
                float4 color = frac(uv.x) > _ComparisonSlider ? color0 : color1;

                return MakePreviewColor(i, _MainTex0_TexelSize.zw, color);
            }
            ENDCG
        }
    }
}
