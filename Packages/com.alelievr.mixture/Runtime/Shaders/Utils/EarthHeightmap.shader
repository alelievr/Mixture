Shader "Hidden/Mixture/EarthHeightmap"
{	
	Properties
	{
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			HLSLPROGRAM
			#include "Packages/com.alelievr.mixture/Runtime/Shaders/MixtureFixed.hlsl"
			#include "Packages/com.alelievr.mixture/Runtime/Shaders/MixtureBlit.hlsl"
            #pragma vertex BlitVertexShader
			#pragma fragment Fragment
			#pragma target 3.0

			#pragma enable_d3d11_debug_symbols

			Texture2D _Heightmap;

			float4 Fragment(Varyings i) : SV_Target
			{
				float4 encodedHeight = SAMPLE_TEXTURE2D(_Heightmap, s_linear_clamp_sampler, i.uv.xy);

				float height = (encodedHeight.r * 256 + encodedHeight.g + encodedHeight.b / 256);

				height /= 64;

				return float4(height.xxx, 1);
			}
			ENDHLSL
		}
	}
}
