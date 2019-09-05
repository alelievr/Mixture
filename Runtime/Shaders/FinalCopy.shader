Shader "Hidden/Mixture/FinalCopy"
{	
	Properties
	{
		_Source_2D("Source", 2D) = "white" {}
		_Source_3D("Source", 3D) = "white" {}
		_Source_Cube("Source", Cube) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM

			#include "MixtureFixed.cginc"
			
            #pragma multi_compile CRT_2D CRT_3D CRT_CUBE

			#pragma fragment mixture
            #pragma vertex CustomRenderTextureVertexShader
			#pragma target 3.0

			TEXTURE_SAMPLER_X(_Source);

			float4 mixture (v2f_customrendertexture i) : SV_Target
			{
				return SAMPLE_X(_Source, i.localTexcoord.xyz, i.direction);
			}
			ENDCG
		}
	}
}
