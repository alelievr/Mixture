Shader "Hidden/Mixture/TextureSample"
{	
	Properties
	{
		[InlineTexture]_Texture_2D("Texture", 2D) = "white" {}
		[InlineTexture]_UV_2D("UV", 2D) = "uv" {}
		
		[InlineTexture]_Texture_3D("Texture", 3D) = "white" {}
		[InlineTexture]_UV_3D("UV", 3D) = "uv" {}
		
		[InlineTexture]_Texture_Cube("Texture", Cube) = "white" {}
		[InlineTexture]_UV_Cube("Direction", Cube) = "uv" {}
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
			#pragma shader_feature _ USE_CUSTOM_UV

			TEXTURE_SAMPLER_X(_Texture);
			TEXTURE_X(_UV);

			float4 mixture (v2f_customrendertexture i) : SV_Target
			{
#ifdef USE_CUSTOM_UV
				float3 uv = SAMPLE_X_NEAREST_CLAMP(_UV, i.localTexcoord.xyz, i.direction).rgb;
#else
				float3 uv = GetDefaultUVs(i);
#endif

				float4 col = SAMPLE_X(_Texture, uv, uv);
				return col;
			}
			ENDHLSL
		}
	}
}
