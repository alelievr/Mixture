Shader "Hidden/Mixture/VolumeToVectorField"
{	
	Properties
	{
		// By default a shader node is supposed to handle all the input texture dimension, we use a prefix to determine which one is used
		[InlineTexture]_Source_3D("Input", 3D) = "white" {}

		// Other parameters
		_Strength("Strength", Float) = 1
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

			float _Strength;

			// This macro will declare a version for each dimention (2D, 3D and Cube)
			TEXTURE_SAMPLER_X(_Source);

			float Sample(float3 uv)
			{
				float4 value = SAMPLE_X(_Source, uv, 0);

				// TODO: channel swizzle parameter
				return value.x;
			}

			float4 mixture (v2f_customrendertexture i) : SV_Target
			{
				float4 voxelSize = float4(rcp(float3(_CustomRenderTextureWidth, _CustomRenderTextureHeight, _CustomRenderTextureDepth)), 0);
				float3 uv = i.localTexcoord.xyz;

				float alpha = SAMPLE_X(_Source, uv, 0).a;

				float3 vec = -float3(
					Sample(uv + voxelSize.xww) - Sample(uv - voxelSize.xww),
					Sample(uv + voxelSize.wyw) - Sample(uv - voxelSize.wyw),
					Sample(uv + voxelSize.wwz) - Sample(uv - voxelSize.wwz)
				);

				return float4(vec * _Strength, alpha);
			}
			ENDHLSL
		}
	}
}
