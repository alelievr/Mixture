Shader "Hidden/CustomRenderTextureMissingMaterial"
{
	Properties
	{
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#include "Packages/com.alelievr.mixture/Runtime/Shaders/MixtureFixed.cginc"
			#pragma fragment MixtureFragment

            #pragma vertex CustomRenderTextureVertexShader
			#pragma target 3.0

			float4 mixture(v2f_customrendertexture IN) : SV_Target
			{
				return float4(1, 0, 1, 1);
			}
			ENDCG
		}
	}
}
