Shader "Hidden/Mixture/PerlinNoise"
{	
	Properties
	{
		[InlineTexture(HideInNodeInspector)] _UV_2D("UVs", 2D) = "white" {}
		[InlineTexture(HideInNodeInspector)] _UV_3D("UVs", 3D) = "white" {}
		[InlineTexture(HideInNodeInspector)] _UV_Cube("UVs", Cube) = "white" {}

		[MixtureVector2]_OutputRange("Output Range", Vector) = (-1, 1, 0, 0)
		_Lacunarity("Lacunarity", Float) = 2
		_Frequency("Frequency", Float) = 5
		_Persistance("Persistance", Float) = 0.5
		[IntRange]_Octaves("Octaves", Range(1, 12)) = 5
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#include "Packages/com.alelievr.mixture/Runtime/Shaders/MixtureFixed.cginc"
			#include "Packages/com.alelievr.mixture/Runtime/Shaders/Noises.hlsl"
			#include "UnityCustomRenderTexture.cginc"
            #pragma vertex CustomRenderTextureVertexShader
			#pragma fragment mixture
			#pragma target 3.0

			// The list of defines that will be active when processing the node with a certain dimension
            #pragma multi_compile CRT_2D CRT_3D CRT_CUBE
			#pragma multi_compile _ USE_CUSTOM_UV

			// This macro will declare a version for each dimention (2D, 3D and Cube)
			TEXTURE_SAMPLER_X(_UV);
			float _Octaves;
			float2 _OutputRange;
			float _Lacunarity;
			float _Frequency;
			float _Persistance;

			float samplePerlinNoise3D(float3 coords)
			{
				return 0;
			}

			float4 mixture (v2f_customrendertexture i) : SV_Target
			{
				// The SAMPLE_X macro handles sampling for 2D, 3D and cube textures
#ifdef USE_CUSTOM_UV
				float3 uvs = SAMPLE_X(_UV, i.localTexcoord.xyz, i.direction);
#else
	#ifdef CRT_CUBE
				float3 uvs = i.direction;
	#else
				float3 uvs = i.localTexcoord.xyz;
	#endif
#endif

#ifdef CRT_2D
				float4 noise = float4(GeneratePerlin2DNoise(uvs, _Frequency, _Octaves, _Persistance, _Lacunarity), 1);
#else
				float4 noise = GeneratePerlin3DNoise(uvs, _Frequency, _Octaves, _Persistance, _Lacunarity);
#endif

				return Remap(noise, -1, 1, _OutputRange.x, _OutputRange.y);
			}
			ENDCG
		}
	}
}
