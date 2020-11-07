Shader "Hidden/Mixture/HSV"
{	
	Properties
	{
		[InlineTexture]_Source_2D("Input", 2D) = "white" {}
		[InlineTexture]_Source_3D("Input", 3D) = "white" {}
		[InlineTexture]_Source_Cube("Input", Cube) = "white" {}

		[Range]_Hue("Hue", Range(0.0,1.0)) = 0.5
		[Range]_Saturation("Saturation", Range(0.0,1.0)) = 0.5
		[Range]_Value("Value", Range(0.0,1.0)) = 0.5
		[Tooltip(For HDR images, you need to specify the maximum value of your image)]_MaxValue("Max Value", Float) = 1.0
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#include "Packages/com.alelievr.mixture/Runtime/Shaders/MixtureFixed.cginc"
            #pragma vertex CustomRenderTextureVertexShader
			#pragma fragment MixtureFragment
			#pragma target 3.0

            #pragma shader_feature CRT_2D CRT_3D CRT_CUBE

			TEXTURE_SAMPLER_X(_Source);

			float _Hue;
			float _Saturation;
			float _Value;
			float _MaxValue;

			float4 mixture(v2f_customrendertexture i) : SV_Target
			{
				float4 source = SAMPLE_X(_Source, i.localTexcoord.xyz, i.direction);
				float3 source_hsv = RGBtoHSV(source);

				source_hsv.x = frac(source_hsv.x + _Hue - 0.5);

				if (_Saturation < 0.5f)
					source_hsv.y = source_hsv.y * (_Saturation * 2);
				else
					source_hsv.y = lerp(source_hsv.y, 1, (_Saturation - 0.5) * 2);

				source.xyz = HSVtoRGB(source_hsv);

				if (_Value < 0.5f)
					source.xyz = source.xyz * (_Value * 2);
				else
					source.xyz = lerp(source.xyz, _MaxValue, (_Value - 0.5) * 2);

				return source;
			}
			ENDCG
		}
	}
}
