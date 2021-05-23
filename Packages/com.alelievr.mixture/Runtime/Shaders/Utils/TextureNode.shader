Shader "Hidden/Mixture/TextureNode"
{	
	Properties
	{
		// By default a shader node is supposed to handle all the input texture dimension, we use a prefix to determine which one is used
		[InlineTexture]_Source_2D("Source", 2D) = "white" {}
		[InlineTexture]_Source_3D("Source", 3D) = "white" {}
		[InlineTexture]_Source_Cube("Source", Cube) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		HLSLINCLUDE

		// Keep in sync with PowerOf2Mode on C# side
		void TransformUVForPOT(int _POTMode, inout float3 uv, inout float3 direction)
		{
			switch (_POTMode)
			{
				default:
				case 0: // None
				case 1: // Scale to next POT
				case 2: // Scale to closest POT
					// no need to change because the scaling is done automatically when sampling UVs
					break;
			}
		}

		ENDHLSL

		Pass
		{
			Name "Normal Map Conversion"

			HLSLPROGRAM
			#include "Packages/com.alelievr.mixture/Runtime/Shaders/MixtureFixed.hlsl"
            #pragma vertex CustomRenderTextureVertexShader
			#pragma fragment MixtureFragment
			#pragma target 3.0

			// The list of defines that will be active when processing the node with a certain dimension
            #pragma shader_feature CRT_2D CRT_3D CRT_CUBE

			// This macro will declare a version for each dimention (2D, 3D and Cube)
			TEXTURE_SAMPLER_X(_Source);
			float _Mip;
			int _POTMode;

			float4 mixture (v2f_customrendertexture i) : SV_Target
			{
				TransformUVForPOT(_POTMode, i.localTexcoord.xyz, i.direction);
				// Move compressed normal map info from AG to RG
				return float4(SAMPLE_LOD_X(_Source, i.localTexcoord.xyz, i.direction, _Mip).ag, 1, 1);
			}
			ENDHLSL
		}

		Pass
		{
			Name "Transform to POT"

			HLSLPROGRAM
			#include "Packages/com.alelievr.mixture/Runtime/Shaders/MixtureFixed.hlsl"
            #pragma vertex CustomRenderTextureVertexShader
			#pragma fragment MixtureFragment
			#pragma target 3.0

			// The list of defines that will be active when processing the node with a certain dimension
            #pragma shader_feature CRT_2D CRT_3D CRT_CUBE

			// This macro will declare a version for each dimention (2D, 3D and Cube)
			TEXTURE_SAMPLER_X(_Source);
			float _Mip;
			int _POTMode;

			float4 mixture (v2f_customrendertexture i) : SV_Target
			{
				TransformUVForPOT(_POTMode, i.localTexcoord.xyz, i.direction);
				return SAMPLE_LOD_X(_Source, i.localTexcoord.xyz, i.direction, _Mip);
			}
			ENDHLSL
		}
	}
}
