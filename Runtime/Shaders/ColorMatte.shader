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
			#pragma vertex vert
			#pragma fragment mixture

			#include "UnityCG.cginc"
			#include "MixtureFixed.cginc"

			float4 _Color;

			float4 mixture (MixtureInputs i) : SV_Target
			{
				return _Color;
			}
			ENDCG
		}
	}
}
