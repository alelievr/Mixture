Shader "Hidden/Mixture/ColorMatte"
{	
	Properties
	{
		[HDR]_Color("Color", Color) = (1.0,0.3,0.1,1.0)
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
			#pragma fragment MixtureFragment
			#pragma target 3.0

			#pragma shader_feature CRT_2D CRT_3D CRT_CUBE

			float4 _Color;

			float4 mixture(v2f_customrendertexture IN) : SV_Target
			{
				return _Color;
			}
			ENDCG
		}
	}
}
