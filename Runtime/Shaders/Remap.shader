Shader "Hidden/Mixture/Remap"
{	
	Properties
	{
		[InlineTexture]_Input_2D("Input", 2D) = "white" {}
		[InlineTexture]_Input_3D("Input", 3D) = "white" {}
		[InlineTexture]_Input_Cube("Input", Cube) = "white" {}

		_Map("Map",2D) = "white" {}

		[Enum(Brightness (Gradient),0,Alpha (Curve),1,Brightness (Curve),2,Saturation (Curve),3,Hue (Curve),4)]_Mode("Mode", Float) = 0
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#include "MixtureFixed.cginc"
			#include "UnityCustomRenderTexture.cginc"
            #pragma vertex CustomRenderTextureVertexShader
			#pragma fragment mixture
			#pragma target 3.0

            #pragma multi_compile CRT_2D CRT_3D CRT_CUBE

			TEXTURE_X(_Input);
			TEXTURE2D(_Map);

			float _Mode;
			
			float4 Remap(float4 sourceValue, uint mode)
			{
				float3 hsv = RGBtoHSV(sourceValue.xyz);

				switch (mode)
				{
				default:
				case 0: // Full RGBA Gradient
					return tex2Dlod(_Map, float4(hsv.zzz,0));
				case 1: // Alpha from Curve
					sourceValue.a = tex2Dlod(_Map, float4(sourceValue.aaa,0)).r;
					return sourceValue;
				case 2: // Brightness from Curve
					hsv.z = tex2Dlod(_Map, float4(hsv.zzz,0)).r;
					return float4(HSVtoRGB(hsv), sourceValue.a);
				case 3: // Saturation from Curve
					hsv.y = tex2Dlod(_Map, float4(hsv.yyy,0)).r;
					return float4(HSVtoRGB(hsv), sourceValue.a);
				case 4: // Hue from Curve
					hsv.x = tex2Dlod(_Map, float4(hsv.xxx,0)).r;
					return float4(HSVtoRGB(hsv), sourceValue.a);
				}
				return 0;
			}

			float4 mixture (v2f_customrendertexture i) : SV_Target
			{
				float4 input = SAMPLE_X(_Input, i.localTexcoord.xyz, i.direction);
				return Remap(input, _Mode);
			}
			ENDCG
		}
	}
}
