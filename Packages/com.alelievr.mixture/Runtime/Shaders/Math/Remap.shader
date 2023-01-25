Shader "Hidden/Mixture/Remap"
{	
	Properties
	{
		[InlineTexture]_Input_2D("Input", 2D) = "white" {}
		[InlineTexture]_Input_3D("Input", 3D) = "white" {}
		[InlineTexture]_Input_Cube("Input", Cube) = "white" {}

		_Map("Map",2D) = "white" {}

		[MixtureRemapMode]_Mode("Mode", Float) = 0
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

			TEXTURE_SAMPLER_X(_Input);
			TEXTURE_SAMPLER2D(_Map);

			float _Mode;
			
			float4 MixtureRemap(float4 sourceValue, uint mode)
			{
				float3 hsv = RGBtoHSV(sourceValue.xyz);

				switch (mode)
				{
				default:
				case 0: // Full RGBA Gradient
					return SAMPLE_2D(_Map, hsv.zzz);
				case 1: // Alpha from Curve
					sourceValue.a = SAMPLE_2D(_Map, sourceValue.aaa).r;
					return sourceValue;
				case 2: // Brightness from Curve
					hsv.z = SAMPLE_2D(_Map, hsv.zzz).r;
					return float4(HSVtoRGB(hsv), sourceValue.a);
				case 3: // Saturation from Curve
					hsv.y = SAMPLE_2D(_Map, hsv.yyy).r;
					return float4(HSVtoRGB(hsv), sourceValue.a);
				case 4: // Hue from Curve
					hsv.x = SAMPLE_2D(_Map, hsv.xxx).r;
					return float4(HSVtoRGB(hsv), sourceValue.a);
				case 5: // Red from Curve
					sourceValue.r = SAMPLE_2D(_Map, sourceValue.rrr).r;
					return sourceValue;
				case 6: // Green from Curve
					sourceValue.g = SAMPLE_2D(_Map, sourceValue.ggg).r;
					return sourceValue;
				case 7: // Blue from Curve
					sourceValue.b = SAMPLE_2D(_Map, sourceValue.bbb).r;
					return sourceValue;
				case 8: // Blue from Curve
					sourceValue.r = SAMPLE_2D(_Map, sourceValue.rrr).r;
					sourceValue.g = SAMPLE_2D(_Map, sourceValue.ggg).g;
					sourceValue.b = SAMPLE_2D(_Map, sourceValue.bbb).b;
					sourceValue.a = SAMPLE_2D(_Map, sourceValue.aaa).a;
					return sourceValue;
				}
				return 0;
			}

			float4 mixture (v2f_customrendertexture i) : SV_Target
			{
				float4 input = SAMPLE_X(_Input, i.localTexcoord.xyz, i.direction);
				return MixtureRemap(input, _Mode);
			}
			ENDHLSL
		}
	}
}
