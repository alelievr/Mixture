Shader "Hidden/MixtureTexture3DPreview"
{
    Properties
    {
        _Texture3D ("Texture", 3D) = "" {}
        _Depth ("Depth", Float) = 0
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
			Cull Back
			ZWrite Off
            ZTest Off
			ZTest LEqual

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

			#include "Packages/com.alelievr.mixture/Editor/Resources/MixturePreview.hlsl"

            UNITY_DECLARE_TEX3D(_Texture3D);
            // SamplerState sampler_Point_Clamp_Texture3D;
            SamplerState sampler_Linear_Clamp_Texture3D;
            float4 _Texture3D_ST;
            float4 _Texture3D_TexelSize;
            float _Depth;

            fixed4 frag (v2f i) : SV_Target
            {
                float4 color = _Texture3D.SampleLevel(sampler_Linear_Clamp_Texture3D, float3(i.uv, _Depth), _PreviewMip) * _Channels;
                // For debug we can use a point sampler: TODO make it an option in the UI
                // float4 color = _Texture3D.Sample(sampler_Point_Clamp_Texture3D, float3(i.uv, _Depth));

                return MakePreviewColor(i, _Texture3D_TexelSize.zw, color);
            }
            ENDCG
        }
    }
}
