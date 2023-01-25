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
			#include "Packages/com.alelievr.mixture/Runtime/Shaders/MixtureSRGB.hlsl"
            #pragma vertex BlitVertexShader
			#pragma fragment Fragment
			#pragma target 3.0

			// #pragma enable_d3d11_debug_symbols

			#define MIN_EARTH_HEIGHT -11000
			#define MAX_EARTH_HEIGHT 8900

			Texture2D _Heightmap;
			float _MinHeight;
			float _MaxHeight;
			float _Mode;
			float _RemapMin;
			float _RemapMax;
			float _Scale;
			float _HeightOffset;

			float4 Fragment(Varyings i) : SV_Target
			{
				float3 encodedHeight = saturate(LOAD_TEXTURE2D(_Heightmap, i.uv.xy * 256).rgb) * 256;

				// Conversion formula: https://www.mapzen.com/blog/elevation/
				float height = (encodedHeight.r * 256 + encodedHeight.g + encodedHeight.b / 256) - 32768;

				height += _HeightOffset;

				switch (_Mode)
				{
					default:
					case 0: // raw:
						return height;
					case 1: // remap:
						return MixtureRemap(height, _MinHeight, _MaxHeight, _RemapMin, _RemapMax);
					case 2: // Scale:
						return height * _Scale;
				}
			}
			ENDHLSL
		}
	}
}
