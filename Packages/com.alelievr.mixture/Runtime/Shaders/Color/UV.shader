Shader "Hidden/Mixture/UV"
{	
	Properties
	{
		[MixtureVector3]_Scale("UV Scale", Vector) = (1.0,1.0,1.0,0.0)
		[MixtureVector3]_Bias("UV Bias", Vector) = (0.0,0.0,0.0,0.0)
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

			float4 _Scale;
			float4 _Bias;

			float4 mixture (v2f_customrendertexture IN) : SV_Target
			{
#ifdef CRT_CUBE
				return float4(IN.direction, 1) ;
#else
				return float4(IN.globalTexcoord.xyz * _Scale.xyz + _Bias.xyz, 1);
#endif
			}
			ENDHLSL
		}
	}
}
