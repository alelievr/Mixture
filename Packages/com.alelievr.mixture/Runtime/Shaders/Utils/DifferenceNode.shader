Shader "Hidden/Mixture/DifferenceNode"
{	
	Properties
	{
		// By default a shader node is supposed to handle all the input texture dimension, we use a prefix to determine which one is used
		[InlineTexture]_A_2D("A", 2D) = "white" {}
		[InlineTexture]_A_3D("A", 3D) = "white" {}
		[InlineTexture]_A_Cube("A", Cube) = "white" {}

		[InlineTexture]_B_2D("B", 2D) = "white" {}
		[InlineTexture]_B_3D("B", 3D) = "white" {}
		[InlineTexture]_B_Cube("B", Cube) = "white" {}

		// Other parameters
		[Enum(Error Diff, 0, Perceptual Diff, 1, Swap, 2, Onion Skin, 3)]_Mode("Mode", Float) = 0

		[VisibleIf(_Mode, 0)]
		_ErrorMultiplier("Error Multiplier", Float) = 1
		
		[VisibleIf(_Mode, 2)]
		[Toggle]_Swap("Swap", Float) = 0

		[VisibleIf(_Mode, 3)]
		_Slide("Slide", Range(0, 1)) = 0.5
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
			TEXTURE_SAMPLER_X(_A);
			TEXTURE_SAMPLER_X(_B);
			float _Mode;
			float _ErrorMultiplier;
			float _Swap;
			float _Slide;

			// Linear RGB to XYZ using D65 ref. white
			float3 RGBtoXYZ(float3 color)
			{
				float x = color.r * 0.4124564 + color.g * 0.3575761 + color.b * 0.1804375;
				float y = color.r * 0.2126729 + color.g * 0.7151522 + color.b * 0.0721750;
				float z = color.r * 0.0193339 + color.g * 0.1191920 + color.b * 0.9503041;
				return float3(x * 100, y * 100, z * 100);
			}

			// sRGB to JzAzBz
			// https://www.osapublishing.org/oe/fulltext.cfm?uri=oe-25-13-15131&id=368272
			float3 RGBtoJAB(float4 color)
			{
				float3 xyz = RGBtoXYZ(color.rgb);

				const float kB  = 1.15;
				const float kG  = 0.66;
				const float kC1 = 0.8359375;        // 3424 / 2^12
				const float kC2 = 18.8515625;       // 2413 / 2^7
				const float kC3 = 18.6875;          // 2392 / 2^7
				const float kN  = 0.15930175781;    // 2610 / 2^14
				const float kP  = 134.034375;       // 1.7 * 2523 / 2^5
				const float kD  = -0.56;
				const float kD0 = 1.6295499532821566E-11;

				float x2 = kB * xyz.x - (kB - 1) * xyz.z;
				float y2 = kG * xyz.y - (kG - 1) * xyz.x;

				float l = 0.41478372 * x2 + 0.579999 * y2 + 0.0146480 * xyz.z;
				float m = -0.2015100 * x2 + 1.120649 * y2 + 0.0531008 * xyz.z;
				float s = -0.0166008 * x2 + 0.264800 * y2 + 0.6684799 * xyz.z;
				l = pow(abs(l / 10000), kN);
				m = pow(abs(m / 10000), kN);
				s = pow(abs(s / 10000), kN);

				// Can we switch to unity.mathematics yet?
				float3 lms = float3(l, m, s);
				float3 a = float3(kC1, kC1, kC1) + kC2 * lms;
				float3 b = 1 + kC3 * lms;
				float3 tmp = a / b; 

				lms.x = pow(abs(tmp.x), kP);
				lms.y = pow(abs(tmp.y), kP);
				lms.z = pow(abs(tmp.z), kP);

				float3 jab = float3(
					0.5 * lms.x + 0.5 * lms.y,
					3.524000 * lms.x + -4.066708 * lms.y + 0.542708 * lms.z,
					0.199076 * lms.x + 1.096799 * lms.y + -1.295875 * lms.z
				);

				jab.x = ((1 + kD) * jab.x) / (1 + kD * jab.x) - kD0;

				return jab;
			}

			float JABDeltaE(float3 v1, float3 v2)
			{
				float c1 = sqrt(v1.y * v1.y + v1.z * v1.z);
				float c2 = sqrt(v2.y * v2.y + v2.z * v2.z);

				float h1 = atan(v1.z / v1.y);
				float h2 = atan(v2.z / v2.y);

				float deltaH = 2 * sqrt(c1 * c2) * sin((h1 - h2) / 2);
				float deltaE = sqrt(pow(abs(v1.x - v2.x), 2) + pow(abs(c1 - c2), 2) + deltaH * deltaH);
				return deltaE;
			}

			float4 PerceptualDiff(float4 a, float4 b)
			{
				return float4(JABDeltaE(RGBtoJAB(a), RGBtoJAB(b)).xxx * 5, 1);
			}

			float4 mixture (v2f_customrendertexture i) : SV_Target
			{
				float3 uv = i.localTexcoord;
				float4 a = SAMPLE_X(_A, i.localTexcoord.xyz, i.direction);
				float4 b = SAMPLE_X(_B, i.localTexcoord.xyz, i.direction);

				switch (_Mode)
				{
					default:
					case 0: // Error diff
						return abs(a - b) * _ErrorMultiplier;
					case 1: // Perceptual diff
						return PerceptualDiff(a, b);
					case 2: // Swap
						return _Swap > 0.5 ? b : a;
					case 3: // Onion Skin
						return uv.x > _Slide ? b : a;
				}
			}
			ENDHLSL
		}
	}
}
