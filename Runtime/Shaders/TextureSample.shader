Shader "Hidden/Mixture/TextureSample"
{	
	Properties
	{
		[MixtureTexture2D]_Texture("Texture", 2D) = "white" {}
		[MixtureTexture2D]_UV("UV", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment mixture

			#include "UnityCG.cginc"
			#define USE_UV
			#include "MixtureFixed.cginc"
			
			TEXTURE2D(_Texture);
			TEXTURE2D(_UV);

			float4 mixture (MixtureInputs i) : SV_Target
			{
				float2 uv = SAMPLE2D_LOD(_UV, i.uv, 0).rg;
				float4 col = SAMPLE2D_LOD(_Texture, uv, 0);
				return col;
			}
			ENDCG
		}
	}
}
