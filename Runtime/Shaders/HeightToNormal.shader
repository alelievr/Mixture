Shader "Hidden/Mixture/HeightToNormal"
{	
	Properties
	{
		[InlineTexture]_Source_2D("Input", 2D) = "white" {}
		[MixtureChannel]_Channel("Height Channel", Float) = 3
		[Range]_Scale("Scale", Range(0.01,2.0)) = 1.0
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#include "MixtureFixed.cginc"
            #pragma vertex CustomRenderTextureVertexShader
			#pragma fragment mixture
			#pragma target 3.0

            #pragma multi_compile CRT_2D

			TEXTURE_SAMPLER_X(_Source);
			float _Channel;
			float _Scale;

			float sampleHeight(float2 uv)
			{
				return SAMPLE_X(_Source, float3(uv, 0), float3(0, 0, 0))[(uint)_Channel];
			}

			float2 gradient(float2 uv, float2 offset)
			{
				float pu = sampleHeight(uv + offset * float2(0.5, 0.0));
				float pv = sampleHeight(uv + offset * float2(0.0, 0.5));
				float nu = sampleHeight(uv - offset * float2(0.5, 0.0));
				float nv = sampleHeight(uv - offset * float2(0.0, 0.5));

				return float2(pu - nu, pv - nv) / offset;
			}

			float4 mixture(v2f_customrendertexture i) : SV_Target
			{
				float2 dduv = 1.0f / float2(_CustomRenderTextureInfo.xy);
				float2 source = gradient(i.localTexcoord.xy, dduv); 

				float3 dx = float3(_Scale, 0.0, source.x);
				float3 dy = float3(0.0, _Scale, source.y);


				return float4(normalize(cross(dx, dy))*0.5+0.5, 1);
			}
			ENDCG
		}
	}
}
