Shader "Hidden/Mixture/FinalCopy"
{	
	Properties
	{
		_Source("Source", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM

			#include "MixtureFixed.cginc"
			#include "UnityCustomRenderTexture.cginc"

			#pragma fragment mixture
            #pragma vertex CustomRenderTextureVertexShader
			#pragma target 3.0

			TEXTURE2D(_Source);

			float4 mixture (v2f_customrendertexture IN) : SV_Target
			{
				return SAMPLE2D_LOD(_Source, IN.localTexcoord.xy, 0);
			}
			ENDCG
		}
	}
}
