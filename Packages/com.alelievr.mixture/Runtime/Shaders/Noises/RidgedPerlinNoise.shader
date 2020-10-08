Shader "Hidden/Mixture/RidgedPerlinNoise"
{	
	Properties
	{
		[InlineTexture(HideInNodeInspector)] _UV_2D("UVs", 2D) = "white" {}
		[InlineTexture(HideInNodeInspector)] _UV_3D("UVs", 3D) = "white" {}
		[InlineTexture(HideInNodeInspector)] _UV_Cube("UVs", Cube) = "white" {}

        [KeywordEnum(None, Tiled)] _TilingMode("Tiling Mode", Float) = 1
		[ShowInInspector][MixtureVector2]_OutputRange("Output Range", Vector) = (0, 1, 0, 0)
		_Lacunarity("Lacunarity", Float) = 2
		_Frequency("Frequency", Float) = 5
		_Persistance("Persistance", Float) = 0.5
		[IntRange]_Octaves("Octaves", Range(1, 12)) = 5
		[ShowInInspector]_Seed("Seed", Int) = 42
		[ShowInInspector][Enum(RRRR, 0, R, 1, RG, 2, RGB, 3, RGBA, 4)]_Channels("Channels", Int) = 0
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#include "Packages/com.alelievr.mixture/Runtime/Shaders/MixtureFixed.cginc"
			#include "Packages/com.alelievr.mixture/Runtime/Shaders/PerlinNoise.hlsl"
            #pragma vertex CustomRenderTextureVertexShader
			#pragma fragment MixtureFragment
			#pragma target 3.0

			// The list of defines that will be active when processing the node with a certain dimension
            #pragma shader_feature CRT_2D CRT_3D CRT_CUBE
			#pragma shader_feature _ USE_CUSTOM_UV
			#pragma shader_feature _TILINGMODE_NONE _TILINGMODE_TILED

			// This macro will declare a version for each dimention (2D, 3D and Cube)
			TEXTURE_SAMPLER_X(_UV);
			float _Octaves;
			float2 _OutputRange;
			float _Lacunarity;
			float _Frequency;
			float _Persistance;
			int _Seed;
			int _Channels;

			float GenerateNoise(v2f_customrendertexture i, int seed)
			{
				float3 uvs = GetNoiseUVs(i, SAMPLE_X(_UV, i.localTexcoord.xyz, i.direction).xyz, seed);

#ifdef CRT_2D
				float noise = GenerateRidgedPerlin2DNoise(uvs, _Frequency, _Octaves, _Persistance, _Lacunarity, seed);
#else
				float noise = GenerateRidgedPerlin3DNoise(uvs, _Frequency, _Octaves, _Persistance, _Lacunarity, seed);
#endif

				return RemapClamp(noise, 0, 1, _OutputRange.x, _OutputRange.y);
			}

			float4 mixture (v2f_customrendertexture i) : SV_Target
			{
				return GenerateNoiseForChannels(i, _Channels, _Seed);
			}
			ENDCG
		}
	}
}
