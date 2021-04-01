Shader "Hidden/MixtureTextureCubePreview"
{
    Properties
    {
        _Cubemap ("Texture", Cube) = "" {}
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

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

			#include "Packages/com.alelievr.mixture/Editor/Resources/MixturePreview.hlsl"

            TextureCube _Cubemap;
            SamplerState sampler_Linear_Clamp_Cubemap;
            float4 _Cubemap_ST;
            float4 _Cubemap_TexelSize;

            float4 frag (v2f i) : SV_Target
            {
                float4 color = _Cubemap.SampleLevel(sampler_Linear_Clamp_Cubemap, normalize(LatlongToDirectionCoordinate(i.uv)), floor(_PreviewMip)) * _Channels;
                return MakePreviewColor(i, _Cubemap_TexelSize.zw, color);
            }
            ENDHLSL
        }
    }
}
