Shader "Hidden/Mixture/Lerp"
{	
	Properties
	{
		_A("A", 2D) = "white" {}
		_B("B", 2D) = "white" {}
		_T("T", 2D) = "white" {}
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

			TEXTURE2D(_A);
			TEXTURE2D(_B);
			TEXTURE2D(_T);

			float4 mixture (MixtureInputs i) : SV_Target
			{
				// Fow now we only use the r chanell, TODO: make it an option
				return lerp(SAMPLE2D_LOD(_A, i.uv, 0), SAMPLE2D_LOD(_B, i.uv, 0), SAMPLE2D_LOD(_T, i.uv, 0).r);
			}
			ENDCG
		}
	}
}
