Shader "Hidden/Mixture/Generate Mip Map"
{	
	Properties
	{
		// Declare your properties here
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
            // #pragma enable_d3d11_debug_symbols // Enable debugging

            #pragma shader_feature CRT_2D CRT_3D CRT_CUBE

			// Built-in parameter to generate the mipmaps
			TEXTURE_X(_PreviousMip);
			float _SourceMip;
			float4 _RcpTextureSize;
			float3 _GaussianBlurDirection;

			// This is an example of how to generate a gaussian blur mip chain, feel free to replace this by your mipmap generation code. 
			float4 Gaussian(float3 uv, float3 direction)
			{
				// Gaussian weights for 9 texel kernel
				const half gaussWeights[] = { 0.27343750, 0.21875000, 0.10937500, 0.03125000, 0.00390625 };

				float3 offset = _RcpTextureSize.xyz * _GaussianBlurDirection;
				float3 offset1 = offset * (1.0 + (gaussWeights[2] / (gaussWeights[1] + gaussWeights[2])));
				float3 offset2 = offset * (3.0 + (gaussWeights[4] / (gaussWeights[3] + gaussWeights[4])));

				float3 uv_m2 = uv.xyz - offset2;
				float3 uv_m1 = uv.xyz - offset1;
				float3 uv_p0 = uv.xyz;
				float3 uv_p1 = uv.xyz + offset1;
				float3 uv_p2 = uv.xyz + offset2;

				// Cubemap direction sampling offset
				float cubemapDirectionOffset = _RcpTextureSize.x * 360 / 5; // rotation anngle in degree divided by number of sample
				float3 dir_m2 = Rotate(offset, direction, cubemapDirectionOffset * -2);
				float3 dir_m1 = Rotate(offset, direction, cubemapDirectionOffset * -1);
				float3 dir_p0 = Rotate(offset, direction, cubemapDirectionOffset * +0);
				float3 dir_p1 = Rotate(offset, direction, cubemapDirectionOffset * +1);
				float3 dir_p2 = Rotate(offset, direction, cubemapDirectionOffset * +2);

				return
					+ SAMPLE_LOD_X_LINEAR_CLAMP(_PreviousMip, uv_m2, dir_m2, _SourceMip) * (gaussWeights[3] + gaussWeights[4])
					+ SAMPLE_LOD_X_LINEAR_CLAMP(_PreviousMip, uv_m1, dir_m1, _SourceMip) * (gaussWeights[1] + gaussWeights[2])
					+ SAMPLE_LOD_X_LINEAR_CLAMP(_PreviousMip, uv_p0, dir_p0, _SourceMip) *  gaussWeights[0]
					+ SAMPLE_LOD_X_LINEAR_CLAMP(_PreviousMip, uv_p1, dir_p1, _SourceMip) * (gaussWeights[1] + gaussWeights[2])
					+ SAMPLE_LOD_X_LINEAR_CLAMP(_PreviousMip, uv_p2, dir_p2, _SourceMip) * (gaussWeights[3] + gaussWeights[4]);
			}

			float4 mixture (v2f_customrendertexture i) : SV_Target
			{
				return Gaussian(i.localTexcoord.xyz, i.direction);
			}
			ENDHLSL
		}
	}
}
