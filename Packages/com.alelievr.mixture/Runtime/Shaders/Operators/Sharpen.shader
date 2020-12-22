Shader "Hidden/Mixture/Sharpen"
{	
	Properties
	{
		// By default a shader node is supposed to handle all the input texture dimension, we use a prefix to determine which one is used
		[InlineTexture]_Source_2D("Source", 2D) = "white" {}
		[InlineTexture]_Source_3D("Source", 3D) = "white" {}
		[InlineTexture]_Source_Cube("Source", Cube) = "white" {}

		_Strength("Strength", Range(0.1, 2)) = 1
	}

	HLSLINCLUDE
	
	#include "Packages/com.alelievr.mixture/Runtime/Shaders/MixtureFixed.hlsl"
	#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

	#pragma target 3.0
	// The list of defines that will be active when processing the node with a certain dimension
	#pragma shader_feature CRT_2D CRT_3D CRT_CUBE
	#pragma vertex CustomRenderTextureVertexShader
	#pragma fragment MixtureFragment

	static float3x3 sharpenMatrix = float3x3(
		 0, -1,  0,
		-1,  5, -1,
		 0, -1,  0
	);

	TEXTURE_SAMPLER_X(_Source);
	float _Strength;

	float4 Sharpen(float4 up, float4 left, float4 right, float4 bottom, float4 center)
	{
		float4 result = 0.0;

		result += sharpenMatrix[0][1] * up;
		result += sharpenMatrix[1][0] * left;
		result += sharpenMatrix[1][1] * center;
		result += sharpenMatrix[1][2] * right;
		result += sharpenMatrix[2][1] * bottom;

		return result;
	}

	ENDHLSL

	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			Name "Sharpen"

			HLSLPROGRAM
			float4 mixture(v2f_customrendertexture i) : SV_Target
			{
				// TODO: cubemaps support

				float4 rcpSize = float4(rcp(float3(_CustomRenderTextureWidth, _CustomRenderTextureHeight, _CustomRenderTextureDepth)), 0);
				return Sharpen(
					SAMPLE_X(_Source, i.localTexcoord.xyz + rcpSize.wyw * _Strength, float3(0, 0, 0)),
					SAMPLE_X(_Source, i.localTexcoord.xyz + rcpSize.xww * _Strength, float3(0, 0, 0)),
					SAMPLE_X(_Source, i.localTexcoord.xyz - rcpSize.xww * _Strength, float3(0, 0, 0)),
					SAMPLE_X(_Source, i.localTexcoord.xyz - rcpSize.wyw * _Strength, float3(0, 0, 0)),
					SAMPLE_X(_Source, i.localTexcoord.xyz + rcpSize.www * _Strength, float3(0, 0, 0))
				);
			}
			ENDHLSL
		}
	}
}
