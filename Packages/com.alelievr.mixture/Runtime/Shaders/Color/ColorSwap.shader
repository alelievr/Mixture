Shader "Hidden/Mixture/ColorSwap"
{	
	Properties
	{
		// By default a shader node is supposed to handle all the input texture dimension, we use a prefix to determine which one is used
		[InlineTexture]_Source_2D("Source", 2D) = "white" {}
		[InlineTexture]_Source_3D("Source", 3D) = "white" {}
		[InlineTexture]_Source_Cube("Source", Cube) = "white" {}

		// Other parameters
		[Tooltip(Color to replace in the image)]_SourceColor("Source Color", Color) = (1, 1, 1, 1)
		[Tooltip(Color that replaces the source color.)]_TargetColor("Target Color", Color) = (0, 0, 0, 0)
		[Tooltip(Tolerance of the test to replace the colors.)]_Threshold("Threshold", Range(0.0001, 5)) = 0.1
		[Tooltip(How sharp the transition between colors are.)]_Feather("Feather", Range(0, 1)) = 0.5
		[Tooltip(Select which color component the comparison will use)]
		[ShowInInspector][Enum(RGBA, 0, Hue, 1, Saturation, 2, Value, 3, Alpha, 4)]_Mode("Color Mode", Float) = 0
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

			// The list of defines that will be active when processing the node with a certain dimension
            #pragma shader_feature CRT_2D CRT_3D CRT_CUBE

			// This macro will declare a version for each dimention (2D, 3D and Cube)
			TEXTURE_SAMPLER_X(_Source);
			float4 _SourceColor;
			float4 _TargetColor;
			float _Threshold;
			float _Feather;
			float _Mode;

			float GetDistanceFromColor(float4 hsv0, float4 hsv1)
			{
				float dh = min(abs(hsv1.x - hsv0.x), 1 - abs(hsv1.x - hsv0.x));
				float ds = abs(hsv1.y - hsv0.y);
				float dv = abs(hsv1.z - hsv0.z);
				float da = abs(hsv1.a - hsv0.a);

				switch (_Mode)
				{
					default:
					case 0: // RGBA
						return sqrt(dh * dh + ds * ds + dv * dv + da * da);
					case 1: // Hue
						return sqrt(dh * dh);
					case 2: // Sat 
						return sqrt(ds * ds);
					case 3: // Val 
						return sqrt(dv * dv);
					case 4: // Alpha
						return sqrt(da * da);
				}
			}

			float4 mixture (v2f_customrendertexture i) : SV_Target
			{
				float4 color = SAMPLE_X(_Source, i.localTexcoord.xyz, i.direction);

				float3 sourceHSV = RGBtoHSV(color.rgb);
				float3 sourceColorHSV = RGBtoHSV(_SourceColor.rgb);

				float dist = GetDistanceFromColor(float4(sourceHSV, color.a), float4(sourceColorHSV, _SourceColor.a));
				float threshold = _Threshold * 0.2f;
				float mask = smoothstep(threshold, _Feather * (threshold - 0.0000001), dist);
				return lerp(color, _TargetColor, mask);
			}
			ENDHLSL
		}
	}
}
