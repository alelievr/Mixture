Shader "Hidden/Mixture/SelfInitialization"
{	
	Properties
	{
		// By default a shader node is supposed to handle all the input texture dimension, we use a prefix to determine which one is used
		[InlineTexture]_InitializationTexture_2D("Source", 2D) = "white" {}
		[InlineTexture]_InitializationTexture_3D("Source", 3D) = "white" {}
		[InlineTexture]_InitializationTexture_Cube("Source", Cube) = "white" {}

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
			TEXTURE_SAMPLER_X(_InitializationTexture);
			float4 _InitializationColor;

			float4 mixture (v2f_customrendertexture i) : SV_Target
			{
				// The SAMPLE_X macro handles sampling for 2D, 3D and cube textures
				return SAMPLE_X(_InitializationTexture, i.localTexcoord.xyz, i.direction) * _InitializationColor;
			}
			ENDHLSL
		}
	}
}
