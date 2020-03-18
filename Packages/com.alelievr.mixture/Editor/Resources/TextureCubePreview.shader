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

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

			#include "Packages/com.alelievr.mixture/Editor/Resources/MixturePreview.hlsl"

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

            UNITY_DECLARE_TEXCUBE(_Cubemap);
            SamplerState sampler_Linear_Clamp_Cubemap;
            float4 _Cubemap_ST;
            float4 _Cubemap_TexelSize;

            fixed4 frag (v2f i) : SV_Target
            {
                float4 color = _Cubemap.SampleLevel(sampler_Linear_Clamp_Cubemap, normalize(LatlongToDirectionCoordinate(i.uv)), 0) * _Channels;
                return MakePreviewColor(i, _Cubemap_TexelSize.zw, color);
            }
            ENDCG
        }
    }
}
