Shader "Hidden/Mixture/Discretize"
{	
	Properties
	{
		// By default a shader node is supposed to handle all the input texture dimension, we use a prefix to determine which one is used
		[InlineTexture]_Source_2D("Source", 2D) = "white" {}
		[InlineTexture]_Source_3D("Source", 3D) = "white" {}
		[InlineTexture]_Source_Cube("Source", Cube) = "white" {}

		_StepCount("Step Count", Int) = 16 

		[ShowInInspector]_Min("Min", Float) = 0
		[ShowInInspector]_Max("Max", Float) = 1
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

			float _StepCount;
			float _Min;
			float _Max;

			// This macro will declare a version for each dimention (2D, 3D and Cube)
			TEXTURE_SAMPLER_X(_Source);

			float4 mixture (v2f_customrendertexture i) : SV_Target
			{
				float4 value = SAMPLE_X(_Source, i.localTexcoord.xyz, i.direction);

				// reamp value
				value = MixtureRemap(value, _Min, _Max, 0, 1);
				// Postirize
				value = round(value * _StepCount) / _StepCount;
				
				return MixtureRemap(value, 0, 1, _Min, _Max);
			}
			ENDHLSL
		}
	}
}
