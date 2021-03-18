Shader "Hidden/Mixture/GenerateMipMaps"
{	
	Properties
	{
		// By default a shader node is supposed to handle all the input texture dimension, we use a prefix to determine which one is used
		[Tooltip(Previous mip texture)][InlineTexture]_PreviousMip_2D("Previous Mip", 2D) = "black" {}
		[Tooltip(Previous mip texture)][InlineTexture]_PreviousMip_3D("Previous Mip", 3D) = "black" {}
		[Tooltip(Previous mip texture)][InlineTexture]_PreviousMip_Cube("Previous Mip", Cube) = "black" {}
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
            #pragma enable_d3d11_debug_symbols

			// The list of defines that will be active when processing the node with a certain dimension
            #pragma shader_feature CRT_2D CRT_3D CRT_CUBE

			// This macro will declare a version for each dimention (2D, 3D and Cube)
			TEXTURE_X(_PreviousMip);
			float _Mode;
			float _SourceMip;
			float4 _RcpTextureSize;
			float3 _GaussianBlurDirection;

			float4 Gaussian(float3 uv, float3 direction)
			{
				// Gaussian weights for 9 texel kernel
				const half gaussWeights[] = { 0.27343750, 0.21875000, 0.10937500, 0.03125000, 0.00390625 };

				// TODO: cubemap handling
				float2 offset = _RcpTextureSize.xyz * _GaussianBlurDirection*2;
				float2 offset1 = offset * (1.0 + (gaussWeights[2] / (gaussWeights[1] + gaussWeights[2])));
				float2 offset2 = offset * (3.0 + (gaussWeights[4] / (gaussWeights[3] + gaussWeights[4])));

				float2 uv_m2 = uv.xy - offset2;
				float2 uv_m1 = uv.xy - offset1;
				float2 uv_p0 = uv.xy;
				float2 uv_p1 = uv.xy + offset1;
				float2 uv_p2 = uv.xy + offset2;

				return
					+ SAMPLE_LOD_X_LINEAR_CLAMP(_PreviousMip, uv_m2, direction, _SourceMip) * (gaussWeights[3] + gaussWeights[4])
					+ SAMPLE_LOD_X_LINEAR_CLAMP(_PreviousMip, uv_m1, direction, _SourceMip) * (gaussWeights[1] + gaussWeights[2])
					+ SAMPLE_LOD_X_LINEAR_CLAMP(_PreviousMip, uv_p0, direction, _SourceMip) *  gaussWeights[0]
					+ SAMPLE_LOD_X_LINEAR_CLAMP(_PreviousMip, uv_p1, direction, _SourceMip) * (gaussWeights[1] + gaussWeights[2])
					+ SAMPLE_LOD_X_LINEAR_CLAMP(_PreviousMip, uv_p2, direction, _SourceMip) * (gaussWeights[3] + gaussWeights[4]);
			}

			float4 Max(float3 uv, float3 direction)
			{
				float3 offset = _RcpTextureSize.xyz * 0.5;

				// TODO: handle texture 3D
				float h1 = SAMPLE_LOD_X_NEAREST_CLAMP(_PreviousMip, uv + float3(offset.x, offset.y, offset.z), direction, _SourceMip);
				float h2 = SAMPLE_LOD_X_NEAREST_CLAMP(_PreviousMip, uv + float3(-offset.x, offset.y, offset.z), direction, _SourceMip);
				float h3 = SAMPLE_LOD_X_NEAREST_CLAMP(_PreviousMip, uv + float3(-offset.x, -offset.y, offset.z), direction, _SourceMip);
				float h4 = SAMPLE_LOD_X_NEAREST_CLAMP(_PreviousMip, uv + float3(offset.x, offset.y, offset.z), direction, _SourceMip);

				return max(max(h1, h2), max(h3, h4));
			}

			float4 mixture (v2f_customrendertexture i) : SV_Target
			{
				switch (_Mode)
				{
					default:
					case 1: // Gaussian
						return Gaussian(i.localTexcoord.xyz, i.direction);
					case 2: // Max
						return Max(i.localTexcoord.xyz, i.direction);
				}
			}
			ENDHLSL
		}
	}
}
