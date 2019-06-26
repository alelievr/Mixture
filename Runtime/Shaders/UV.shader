Shader "Hidden/Mixture/UV"
{	
	Properties
	{
		[MixtureVector2]_Scale("UV Scale", Vector) = (1.0,1.0,0.0,0.0)
		[MixtureVector2]_Bias("UV Bias", Vector) = (0.0,0.0,0.0,0.0)
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
			#define CUSTOM_VS
			#include "MixtureFixed.cginc"

			float4 _Scale;
			float4 _Bias;

			MixtureInputs vert (appdata v)
			{
				MixtureInputs o = InitializeMixtureInputs(v);
				o.uv = (o.uv * _Scale.xy) + _Bias.xy;
				return o;
			}

			float4 mixture (MixtureInputs i) : SV_Target
			{
				return float4(i.uv.x,i.uv.y,0,1);
			}
			ENDCG
		}
	}
}
