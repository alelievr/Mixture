Shader "Hidden/Mixture/PerlinNoise"
{	
	Properties
	{
		[InlineTexture(HideInNodeInspector)] _UV_2D("UVs", 2D) = "white" {}
		[InlineTexture(HideInNodeInspector)] _UV_3D("UVs", 3D) = "white" {}
		[InlineTexture(HideInNodeInspector)] _UV_Cube("UVs", Cube) = "white" {}

        [KeywordEnum(None, Tiled)] _TilingMode("Tiling Mode", Float) = 0
		[MixtureVector2]_OutputRange("Output Range", Vector) = (-1, 1, 0, 0)
		_Lacunarity("Lacunarity", Float) = 2
		_Frequency("Frequency", Float) = 5
		_Persistance("Persistance", Float) = 0.5
		[IntRange]_Octaves("Octaves", Range(1, 12)) = 5
		_Seed("Seed", Int) = 42
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			float _Frequency;
			#include "Packages/com.alelievr.mixture/Runtime/Shaders/MixtureFixed.cginc"
			#include "Packages/com.alelievr.mixture/Runtime/Shaders/Noises.hlsl"
			#include "UnityCustomRenderTexture.cginc"
            #pragma vertex CustomRenderTextureVertexShader
			#pragma fragment mixture
			#pragma target 3.0

			// The list of defines that will be active when processing the node with a certain dimension
            #pragma multi_compile CRT_2D CRT_3D CRT_CUBE
			#pragma multi_compile _ USE_CUSTOM_UV
			#pragma multi_compile _TILINGMODE_NONE _TILINGMODE_TILED

			// This macro will declare a version for each dimention (2D, 3D and Cube)
			TEXTURE_SAMPLER_X(_UV);
			float _Octaves;
			float2 _OutputRange;
			float _Lacunarity;
			float _Persistance;
			int _Seed;

			float4 mixture (v2f_customrendertexture i) : SV_Target
			{
				float3 uvs = GetNoiseUVs(i, SAMPLE_X(_UV, i.localTexcoord.xyz, i.direction), _Seed);

#ifdef CRT_2D
				float3 noise = GeneratePerlin2DNoise(uvs, _Frequency, _Octaves, _Persistance, _Lacunarity).rrr;
#else
				float3 noise = GeneratePerlin3DNoise(uvs, _Frequency, _Octaves, _Persistance, _Lacunarity).rrr;
#endif

				return float4(Remap(noise, -1, 1, _OutputRange.x, _OutputRange.y), 1);
			}
			ENDCG
		}
	}
}
