Shader "Hidden/MixtureTexture2DPreview"
{
    Properties
    {
        _MainTex ("_MainTex", 2D) = "" {}
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

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _MainTex_TexelSize;
            float4 _Size;

            float4 frag (v2f i) : SV_Target
            {
				float4 color = tex2Dlod(_MainTex, float4(i.uv, 0.0, floor(_PreviewMip))) * _Channels;
                return MakePreviewColor(i, _MainTex_TexelSize.zw, color);
            }
            ENDCG
        }
    }
}
