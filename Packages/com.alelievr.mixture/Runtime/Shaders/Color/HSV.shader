Shader "Hidden/Mixture/HSV"
{	
	Properties
	{
		[InlineTexture]_Source_2D("Input", 2D) = "white" {}
		[InlineTexture]_Source_3D("Input", 3D) = "white" {}
		[InlineTexture]_Source_Cube("Input", Cube) = "white" {}

		[InlineTexture]_HSVOffset_2D("HSV Offset", 2D) = "black" {}
		[InlineTexture]_HSVOffset_3D("HSV Offset", 3D) = "black" {}
		[InlineTexture]_HSVOffset_Cube("HSV Offset", Cube) = "black" {}

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
			HLSLPROGRAM
			#include "Packages/com.alelievr.mixture/Runtime/Shaders/MixtureFixed.hlsl"
            #pragma vertex CustomRenderTextureVertexShader
			#pragma fragment MixtureFragment
			#pragma target 3.0

            #pragma shader_feature CRT_2D CRT_3D CRT_CUBE

			TEXTURE_SAMPLER_X(_Source);
			TEXTURE_SAMPLER_X(_HSVOffset);

			float _Hue;
			float _Saturation;
			float _Value;
			float _MaxValue;

			float4 mixture(v2f_customrendertexture i) : SV_Target
			{
				float4 source = SAMPLE_X(_Source, i.localTexcoord.xyz, i.direction);
				float4 offset = SAMPLE_X(_HSVOffset, i.localTexcoord.xyz, i.direction);
				float3 source_hsv = RGBtoHSV(source.rgb);

				float3 hsvOffset = float3(_Hue, _Saturation, _Value) + offset.xyz;

				source_hsv.x = frac(source_hsv.x + hsvOffset.x - 0.5);

				if (hsvOffset.y < 0.5f)
					source_hsv.y = source_hsv.y * (hsvOffset.y * 2);
				else
					source_hsv.y = lerp(source_hsv.y, 1, (hsvOffset.y - 0.5) * 2);

				source.xyz = HSVtoRGB(source_hsv);

				if (hsvOffset.z < 0.5f)
					source.xyz = source.xyz * (hsvOffset.z * 2);
				else
					source.xyz = lerp(source.xyz, _MaxValue, (hsvOffset.z - 0.5) * 2);

				return source;
			}
			ENDHLSL
		}
	}
}
