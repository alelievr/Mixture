Shader "Hidden/Mixture/Threshold"
{	
	Properties
	{
		[InlineTexture]_Source_2D("Input", 2D) = "white" {}
		[InlineTexture]_Source_3D("Input", 3D) = "white" {}
		[InlineTexture]_Source_Cube("Input", Cube) = "white" {}

		[MixtureChannel]_Channel("Channel", Float) = 3

		_Threshold("Threshold", Float) = 0.3333
		_Feather("Feather", Float) = 0.01

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
			#pragma fragment mixture
			#pragma target 3.0

            #pragma multi_compile CRT_2D CRT_3D CRT_CUBE

			TEXTURE_SAMPLER_X(_Source);

			float _Threshold;
			float _Feather;
			float _Channel;

			float ChannelMask(float4 sourceValue, uint mode)
			{
				switch (mode)
				{
				case 0: return sourceValue.x;
				case 1: return sourceValue.y;
				case 2: return sourceValue.z;
				default:
				case 3: return sourceValue.w;
				}
				return 0;
			}

			float4 mixture (v2f_customrendertexture i) : SV_Target
			{
				float4 source = SAMPLE_X(_Source, i.localTexcoord.xyz, i.direction);

				float a = ChannelMask(source, _Channel);
				float f = _Feather * 0.5;
				a = smoothstep(_Threshold - f, _Threshold + f, a);
				return float4(a, a, a, a);
			}
			ENDCG
		}
	}
}
