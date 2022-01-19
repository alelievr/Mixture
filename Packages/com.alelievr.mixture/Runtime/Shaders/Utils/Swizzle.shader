Shader "Hidden/Mixture/Swizzle"
{	
	Properties
	{
		[InlineTexture]_SourceA_2D("Input A", 2D) = "white" {}
		[InlineTexture]_SourceA_3D("Input A", 3D) = "white" {}
		[InlineTexture]_SourceA_Cube("Input A", Cube) = "white" {}

		[InlineTexture]_SourceB_2D("Input B", 2D) = "white" {}
		[InlineTexture]_SourceB_3D("Input B", 3D) = "white" {}
		[InlineTexture]_SourceB_Cube("Input B", Cube) = "white" {}

		[InlineTexture]_SourceC_2D("Input C", 2D) = "white" {}
		[InlineTexture]_SourceC_3D("Input C", 3D) = "white" {}
		[InlineTexture]_SourceC_Cube("Input C", Cube) = "white" {}

		[InlineTexture]_SourceD_2D("Input D", 2D) = "white" {}
		[InlineTexture]_SourceD_3D("Input D", 3D) = "white" {}
		[InlineTexture]_SourceD_Cube("Input D", Cube) = "white" {}

		[MixtureSwizzle]_RMode("Output Red", Float) = 0
		[MixtureSwizzle]_GMode("Output Green", Float) = 1
		[MixtureSwizzle]_BMode("Output Blue", Float) = 2
		[MixtureSwizzle]_AMode("Output Alpha", Float) = 3

		[HDR]_Custom("Custom", Color) = (1.0,1.0,1.0,1.0)
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			HLSLPROGRAM
			#include "Packages/com.alelievr.mixture/Runtime/Shaders/MixtureFixed.hlsl"
            #pragma vertex CustomRenderTextureVertexShader
			#pragma fragment MixtureFragment
			#pragma target 3.0

            #pragma shader_feature CRT_2D CRT_3D CRT_CUBE

			TEXTURE_SAMPLER_X(_SourceA);
			TEXTURE_SAMPLER_X(_SourceB);
			TEXTURE_SAMPLER_X(_SourceC);
			TEXTURE_SAMPLER_X(_SourceD);

			float _RMode;
			float _GMode;
			float _BMode;
			float _AMode;
			float4 _Custom;

			float4 mixture (v2f_customrendertexture i) : SV_Target
			{
				float4 sourceA = SAMPLE_X(_SourceA, i.localTexcoord.xyz, i.direction);
				float4 sourceB = SAMPLE_X(_SourceB, i.localTexcoord.xyz, i.direction);
				float4 sourceC = SAMPLE_X(_SourceC, i.localTexcoord.xyz, i.direction);
				float4 sourceD = SAMPLE_X(_SourceD, i.localTexcoord.xyz, i.direction);

				float r = Swizzle(sourceA, sourceB, sourceC, sourceD, _RMode, _Custom.r);
				float g = Swizzle(sourceA, sourceB, sourceC, sourceD, _GMode, _Custom.g);
				float b = Swizzle(sourceA, sourceB, sourceC, sourceD, _BMode, _Custom.b);
				float a = Swizzle(sourceA, sourceB, sourceC, sourceD, _AMode, _Custom.a);
				return float4(r, g, b, a);
			}
			ENDHLSL
		}
	}
}
