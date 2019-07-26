Shader "Hidden/Mixture/ColorMatte"
{	
	Properties
	{
		_Color("Color", Color) = (1.0,0.3,0.1,1.0)
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

			float4 _Color;

			float4 mixture (v2f_customrendertexture IN) : SV_Target
			{
				return _Color;
			}
			ENDCG
		}
	}
}
