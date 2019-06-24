Shader "Hidden/Mixture/ScaleBias"
{	
	Properties
	{
		[MixtureTexture2D]_Texture("Texture", 2D) = "white" {}
		[Enum(ScaleBias,0,BiasScale,1,Scale,2,Bias,3)]_Mode("Mode", Float) = 0
		_Scale("Scale", Vector) = (1.0,1.0,0.0,0.0)
		_Bias("Bias", Vector) = (0.0,0.0,0.0,0.0)
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
			float _Mode;
			float4 _Scale;
			float4 _Bias;

			float4 mixture (MixtureInputs i) : SV_Target
			{
				float4 col = SAMPLE2D_LOD(_Texture, i.uv, 0);
				uint mode = (uint)_Mode;
				switch (mode)
				{
				case 0: col = (col * _Scale) + _Bias; break;
				case 1: col = (col + _Bias) + _Scale; break;
				case 2: col = col * _Scale; break;
				case 3: col = col + _Bias; break;
				}
				return col;
			}
			ENDCG
		}
	}
}
