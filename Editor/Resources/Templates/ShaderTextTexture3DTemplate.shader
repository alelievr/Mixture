Shader "CustomTexture/ShaderTextTexture3DTemplate"
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

            #pragma shader_feature CRT_2D CRT_3D CRT_CUBE

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
