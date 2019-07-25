﻿Shader "CustomTexture/ShaderTextTextureCubeTemplate"
{	
	Properties
	{
		_Input("Input", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma fragment mixture

			#include "UnityCustomRenderTexture.cginc"
            #pragma vertex CustomRenderTextureVertexShader
			#pragma target 3.0

            sampler2D _Input;

			float4 mixture(v2f_customrendertexture IN) : SV_Target
			{
				return tex2D(_Input, IN.localTexcoord.xy);
			}
			ENDCG
		}
	}
}
