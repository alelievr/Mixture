Shader "Hidden/Mixture/UVDistort"
{	
	Properties
	{
		[InlineTexture] _Texture_2D("Distort Map", 2D) = "white" {}
		[InlineTexture]_UV_2D("UV", 2D) = "white" {}

		[InlineTexture]_Texture_3D("Distort Map", 3D) = "white" {}
		[InlineTexture]_UV_3D("UV", 3D) = "white" {}

		[InlineTexture]_Texture_Cube("Distort Map", Cube) = "white" {}
		[InlineTexture]_UV_Cube("Direction", Cube) = "white" {}

		[MixtureVector3]_Scale("Distort Scale", Vector) = (1.0,1.0,1.0,0.0)
		[MixtureVector3]_Bias("Distort Bias", Vector) = (0.0,0.0,0.0,0.0)
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#include "MixtureFixed.cginc"
			#pragma vertex CustomRenderTextureVertexShader
			#pragma fragment mixture
			#pragma target 3.0

			#pragma multi_compile CRT_2D CRT_3D CRT_CUBE

			TEXTURE_SAMPLER_X(_Texture);
			TEXTURE_SAMPLER_X(_UV);
			float4 _Scale;
			float4 _Bias;

			float4 mixture (v2f_customrendertexture IN) : SV_Target
			{
				float3 uv = SAMPLE_X(_UV, float3(IN.localTexcoord.xy, 0), IN.direction).rgb;
				uv += (SAMPLE_X(_Texture, float3(IN.localTexcoord.xy, 0), IN.direction).rgb + _Bias.xyz) * _Scale.xyz;
				return float4(uv,1);

			}
			ENDCG
		}
	}
}
