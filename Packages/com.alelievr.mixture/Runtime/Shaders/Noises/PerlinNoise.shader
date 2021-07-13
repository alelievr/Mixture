Shader "Hidden/Mixture/PerlinNoise"
{	
	Properties
	{
		[InlineTexture(HideInNodeInspector)] _UV_2D("UVs", 2D) = "uv" {}
		[InlineTexture(HideInNodeInspector)] _UV_3D("UVs", 3D) = "uv" {}
		[InlineTexture(HideInNodeInspector)] _UV_Cube("UVs", Cube) = "uv" {}

        [KeywordEnum(None, Tiled)] _TilingMode("Tiling Mode", Float) = 1
		[ShowInInspector][Enum(2D, 0, 3D, 1)]_UVMode("UV Mode", Float) = 0
		[ShowInInspector][MixtureVector2]_OutputRange("Output Range", Vector) = (-1, 1, 0, 0)
		[ShowInInspector]_Lacunarity("Lacunarity", Float) = 2
		_Frequency("Frequency", Float) = 5
		_Persistance("Persistance", Float) = 0.5
		[IntRange]_Octaves("Octaves", Range(1, 12)) = 5
		_Seed("Seed", Int) = 42
		[Tooltip(Select how many noise to genereate and on which channel. The more different channel you use the more expensive it is (max 4 noise evaluation).)]
		[ShowInInspector][Enum(RRRR, 0, R, 1, RG, 2, RGB, 3, RGBA, 4)]_Channels("Channels", Int) = 0
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			HLSLPROGRAM
			#include "Packages/com.alelievr.mixture/Runtime/Shaders/MixtureFixed.hlsl"
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
			float _Frequency;
			float _Octaves;
			float2 _OutputRange;
			float _Lacunarity;
			float _Persistance;
			int _Seed;
			int _Channels;
			int _UVMode;

			float GenerateNoise(v2f_customrendertexture i, int seed)
			{
				float3 uvs = GetNoiseUVs(i, SAMPLE_X(_UV, i.localTexcoord.xyz, i.direction), seed);

				float3 noise = 0;
#ifdef CRT_2D
				if (_UVMode == 0)
					noise = GeneratePerlin2DNoise(uvs.xy, _Frequency, _Octaves, _Persistance, _Lacunarity, seed);
				else
#endif
					noise = GeneratePerlin3DNoise(uvs, _Frequency, _Octaves, _Persistance, _Lacunarity, seed).x;

				return RemapClamp(noise.x, -1, 1, _OutputRange.x, _OutputRange.y);
			}

			float4 mixture (v2f_customrendertexture i) : SV_Target
			{
				SetupNoiseTiling(_Lacunarity, _Frequency);
				return GenerateNoiseForChannels(i, _Channels, _Seed);
			}
			ENDHLSL
		}
	}
}
