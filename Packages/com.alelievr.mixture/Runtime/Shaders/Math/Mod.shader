Shader "Hidden/Mixture/Mod"
{	
	Properties
	{
		// By default a shader node is supposed to handle all the input texture dimension, we use a prefix to determine which one is used
		[InlineTexture]_SourceA_2D("Source A", 2D) = "white" {}
		[InlineTexture]_SourceA_3D("Source A", 3D) = "white" {}
		[InlineTexture]_SourceA_Cube("Source A", Cube) = "white" {}

		[InlineTexture]_SourceB_2D("Source B", 2D) = "white" {}
		[InlineTexture]_SourceB_3D("Source B", 3D) = "white" {}
		[InlineTexture]_SourceB_Cube("Source B", Cube) = "white" {}

		// Other parameters
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
			TEXTURE_SAMPLER_X(_SourceA);
			TEXTURE_SAMPLER_X(_SourceB);

			float4 mixture (v2f_customrendertexture i) : SV_Target
			{
				// The SAMPLE_X macro handles sampling for 2D, 3D and cube textures
				return SAMPLE_X(_SourceA, i.localTexcoord.xyz, i.direction) % SAMPLE_X(_SourceB, i.localTexcoord.xyz, i.direction);
			}
			ENDHLSL
		}
	}
}
