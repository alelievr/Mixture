Shader "Hidden/Mixture/Combine"
{	
	Properties
	{
		// By default a shader node is supposed to handle all the input texture dimension, we use a prefix to determine which one is used
		[Tooltip(Source Texture for the R channel)][InlineTexture]_SourceR_2D("Source R", 2D) = "black" {}
		[Tooltip(Source Texture for the R channel)][InlineTexture]_SourceR_3D("Source R", 3D) = "black" {}
		[Tooltip(Source Texture for the R channel)][InlineTexture]_SourceR_Cube("Source R", Cube) = "black" {}

		[Tooltip(Source Texture for the G channel)][InlineTexture]_SourceG_2D("Source G", 2D) = "black" {}
		[Tooltip(Source Texture for the G channel)][InlineTexture]_SourceG_3D("Source G", 3D) = "black" {}
		[Tooltip(Source Texture for the G channel)][InlineTexture]_SourceG_Cube("Source G", Cube) = "black" {}

		[Tooltip(Source Texture for the B channel)][InlineTexture]_SourceB_2D("Source B", 2D) = "black" {}
		[Tooltip(Source Texture for the B channel)][InlineTexture]_SourceB_3D("Source B", 3D) = "black" {}
		[Tooltip(Source Texture for the B channel)][InlineTexture]_SourceB_Cube("Source B", Cube) = "black" {}

		[Tooltip(Source Texture for the A channel)][InlineTexture]_SourceA_2D("Source A", 2D) = "black" {}
		[Tooltip(Source Texture for the A channel)][InlineTexture]_SourceA_3D("Source A", 3D) = "black" {}
		[Tooltip(Source Texture for the A channel)][InlineTexture]_SourceA_Cube("Source A", Cube) = "black" {}

		[Tooltip(Select which channel from the R input texture to write in the R output channel)][MixtureSwizzle]_CombineModeR("Output R", Float) = 0
		[Tooltip(Select which channel from the G input texture to write in the G output channel)][MixtureSwizzle]_CombineModeG("Output G", Float) = 1
		[Tooltip(Select which channel from the B input texture to write in the B output channel)][MixtureSwizzle]_CombineModeB("Output B", Float) = 2
		[Tooltip(Select which channel from the A input texture to write in the A output channel)][MixtureSwizzle]_CombineModeA("Output A", Float) = 3

		[HDR]_Custom("Custom", Color) = (1.0, 1.0, 1.0, 1.0)
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

			// The list of defines that will be active when processing the node with a certain dimension
            #pragma shader_feature CRT_2D CRT_3D CRT_CUBE

			// This macro will declare a version for each dimention (2D, 3D and Cube)
			TEXTURE_SAMPLER_X(_SourceR);
			TEXTURE_SAMPLER_X(_SourceG);
			TEXTURE_SAMPLER_X(_SourceB);
			TEXTURE_SAMPLER_X(_SourceA);

			float _CombineModeR;
			float _CombineModeG;
			float _CombineModeB;
			float _CombineModeA;
			float4 _Custom;

			float4 mixture (v2f_customrendertexture i) : SV_Target
			{
				float4 r = SAMPLE_X(_SourceR, i.localTexcoord.xyz, i.direction);
				float4 g = SAMPLE_X(_SourceG, i.localTexcoord.xyz, i.direction);
				float4 b = SAMPLE_X(_SourceB, i.localTexcoord.xyz, i.direction);
				float4 a = SAMPLE_X(_SourceA, i.localTexcoord.xyz, i.direction);

				return float4(
					Swizzle(r, _CombineModeR, _Custom.r),
					Swizzle(g, _CombineModeG, _Custom.g),
					Swizzle(b, _CombineModeB, _Custom.b),
					Swizzle(a, _CombineModeA, _Custom.a)
				);
			}
			ENDHLSL
		}
	}
}
