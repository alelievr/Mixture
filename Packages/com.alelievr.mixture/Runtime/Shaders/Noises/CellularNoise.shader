Shader "Hidden/Mixture/CellularNoise"
{	
	Properties
	{
		[Tooltip(Custom Noise UV)][InlineTexture(HideInNodeInspector)] _UV_2D("UVs", 2D) = "uv" {}
		[Tooltip(Custom Noise UV)][InlineTexture(HideInNodeInspector)] _UV_3D("UVs", 3D) = "uv" {}
		[Tooltip(Custom Noise UV)][InlineTexture(HideInNodeInspector)] _UV_Cube("UVs", Cube) = "uv" {}

        [KeywordEnum(None, Tiled)] _TilingMode("Tiling Mode", Float) = 1
		[ShowInInspector][Enum(2D, 0, 3D, 1)]_UVMode("UV Mode", Float) = 0
		[ShowInInspector][Enum(Euclidean, 0, Manhattan, 1, Minkowski, 2, Triangle, 3)] _DistanceMode("Distance Mode", Float) = 0
		[ShowInInspector][MixtureVector2]_OutputRange("Output Range", Vector) = (0, 1, 0, 0)
		[ShowInInspector]_Lacunarity("Lacunarity", Float) = 2
		
		_Frequency("Frequency", Float) = 5
		_Persistance("Persistance", Float) = 0.3
		[IntRange]_Octaves("Octaves", Range(1, 12)) = 3
		[Tooltip(Act as a multiplier for the distance function)][ShowInInspector]_CellSize("Cell Size", Float) = 1
		_Seed("Seed", Int) = 42
		[Tooltip(Select how many noise to genereate and on which channel. The more different channel you use the more expensive it is (max 4 noise evaluation).)]
		[ShowInInspector][Enum(RRRR, 0, R, 1, RG, 2, RGB, 3, RGBA, 4)]_Channels("Channels", Int) = 0
		[ShowInInspector][Enum(Cell Distance, 0, Smooth Cell Distance, 3, Cells, 1, Valleys, 2)] _CellsModeR("Cells Mode R", Float) = 0
		[ShowInInspector][VisibleIf(_Channels, 2, 3, 4)][Enum(Cell Distance, 0, Smooth Cell Distance, 3, Cells, 1, Valleys, 2)] _CellsModeG("Cells Mode G", Float) = 0
		[ShowInInspector][VisibleIf(_Channels, 3, 4)][Enum(Cell Distance, 0, Smooth Cell Distance, 3, Cells, 1, Valleys, 2)] _CellsModeB("Cells Mode B", Float) = 0
		[ShowInInspector][VisibleIf(_Channels, 4)][Enum(Cell Distance, 0, Smooth Cell Distance, 3, Cells, 1, Valleys, 2)] _CellsModeA("Cells Mode A", Float) = 0
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			HLSLPROGRAM
			float _DistanceMode;
			float _CellSize;
			float _CellsModeR;
			float _CellsModeG;
			float _CellsModeB;
			float _CellsModeA;
			int _Channels;

			#include "Packages/com.alelievr.mixture/Runtime/Shaders/MixtureFixed.hlsl"
			#define CUSTOM_DISTANCE _DistanceMode
			#define CUSTOM_DISTANCE_MULTIPLIER _CellSize
			#include "Packages/com.alelievr.mixture/Runtime/Shaders/CellularNoise.hlsl"
			#include "Packages/com.alelievr.mixture/Runtime/Shaders/NoiseUtils.hlsl"
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
			int _UVMode;

			float4 GenerateCellularNoise(v2f_customrendertexture i, int seed)
			{
				float3 uvs = GetNoiseUVs(i, SAMPLE_X(_UV, i.localTexcoord.xyz, i.direction), seed);

				// TODO: if uv mode is 3D, then sample in 3D
				float4 noise = 0;
#ifdef CRT_2D
				if (_UVMode == 0)
					noise = GenerateCellular2DNoise(uvs.xy, _Frequency, _Octaves, _Persistance, _Lacunarity, seed);
				else // 3D forced by uv mode
#endif
					noise = GenerateCellular3DNoise(uvs, _Frequency, _Octaves, _Persistance, _Lacunarity, seed);

				return RemapClamp(noise, 0, 1, _OutputRange.x, _OutputRange.y);
			}

			float4 mixture (v2f_customrendertexture i) : SV_Target
			{
				SetupNoiseTiling(_Lacunarity, _Frequency);
				return GenerateCellularNoiseForChannels(i, _Seed);
			}
			ENDHLSL
		}
	}
}
