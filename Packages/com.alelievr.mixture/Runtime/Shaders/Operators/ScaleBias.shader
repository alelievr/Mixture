Shader "Hidden/Mixture/ScaleBias"
{	
	Properties
	{
		[InlineTexture]_Texture_2D("Texture", 2D) = "white" {}
		[InlineTexture]_Texture_3D("Texture", 3D) = "white" {}
		[InlineTexture]_Texture_Cube("Texture", Cube) = "white" {}

		[ScaleBias]_Mode("Mode", Float) = 0
		[MixtureVector3]_Scale("Scale", Vector) = (1.0,1.0,1.0,0.0)
		[MixtureVector3]_Bias("Bias", Vector) = (0.0,0.0,0.0,0.0)
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			HLSLPROGRAM
			#include "Packages/com.alelievr.mixture/Runtime/Shaders/MixtureFixed.hlsl"
            #pragma vertex CustomRenderTextureVertexShader
			#pragma fragment MixtureFragment
			#pragma target 3.0

            #pragma shader_feature CRT_2D CRT_3D CRT_CUBE

			TEXTURE_SAMPLER_X(_Texture);
			float _Mode;
			float4 _Scale;
			float4 _Bias;

			float4 mixture (v2f_customrendertexture i) : SV_Target
			{
				float4 col = SAMPLE_X(_Texture, i.localTexcoord, i.direction);

				switch ((uint)_Mode)
				{
					case 0: col = ScaleBias(col, _Scale, _Bias); break;
					case 1: col = BiasScale(col, _Scale, _Bias); break;
					case 2: col = ScaleBias(col, _Scale, 0); break;
					case 3: col = ScaleBias(col, 1, _Bias); break;
					case 4: col = ScaleBias(col, 2, -1); break;
					case 5: col = ScaleBias(col, 0.5, 0.5); break;
				}
				return col;
			}
			ENDHLSL
		}
	}
}
