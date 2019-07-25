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
			CGPROGRAM
			#pragma fragment mixture

			#include "MixtureFixed.cginc"
			#include "UnityCustomRenderTexture.cginc"

            #pragma vertex CustomRenderTextureVertexShader
			#pragma target 3.0

			float4 _Scale;
			float4 _Bias;

			float4 mixture (v2f_customrendertexture IN) : SV_Target
			{
				return float4(1, 1, 0, 1);//float4(IN.globalTexcoord.xyz, 1) * _Scale + _Bias;
			}
			ENDCG
		}
	}
}
