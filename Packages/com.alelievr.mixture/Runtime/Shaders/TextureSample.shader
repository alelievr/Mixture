Shader "Hidden/Mixture/TextureSample"
{	
	Properties
	{
		[InlineTexture]_Texture_2D("Texture", 2D) = "white" {}
		[InlineTexture]_UV_2D("UV", 2D) = "white" {}
		
		[InlineTexture]_Texture_3D("Texture", 3D) = "white" {}
		[InlineTexture]_UV_3D("UV", 3D) = "white" {}
		
		[InlineTexture]_Texture_Cube("Texture", Cube) = "white" {}
		[InlineTexture]_UV_Cube("Direction", Cube) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM		
			#include "Packages/com.alelievr.mixture/Runtime/Shaders/MixtureFixed.cginc"
            #pragma vertex CustomRenderTextureVertexShader
			#pragma fragment MixtureFragment
			#pragma target 3.0

			#pragma shader_feature CRT_2D CRT_3D CRT_CUBE

			TEXTURE_SAMPLER_X(_Texture);
			TEXTURE_SAMPLER_X(_UV);

			float4 mixture (v2f_customrendertexture i) : SV_Target
			{
				float3 uv = SAMPLE_X(_UV, float3(i.localTexcoord.xy, 0), i.direction).rgb;
				float4 col = SAMPLE_X(_Texture, uv, uv);
				return col;
			}
			ENDCG
		}
	}
}
