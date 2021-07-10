Shader "Hidden/Mixture/VectorField"
{	
	Properties
	{
		[InlineTexture]_UV_2D("UV", 2D) = "white" {}
		[InlineTexture]_UV_3D("UV", 3D) = "white" {}
		[InlineTexture]_UV_Cube("UV", Cube) = "white" {}

		[Enum(Direction, 0, Circular, 1, Stripes, 2, Turbulence, 3)]_Mode("Mode", Float) = 0

		[VisibleIf(_Mode, 0)][MixtureVector3]_Direction("Direction", Vector) = (1, 0, 0, 0)

		[VisibleIf(_Mode, 1)]_PointInwards("Point Inwards", Range(-1, 1)) = 0.2

		[VisibleIf(_Mode, 2)]_StripeCount("Stripe Count", Int) = 10
		[VisibleIf(_Mode, 2)]_Randomness("Randomness", Range(0, 1)) = 0
		[VisibleIf(_Mode, 2, 3)]_Seed("Seed", Int) = 42

		[VisibleIf(_Mode, 3)]_Frequency("Frequency", Int) = 6
		[VisibleIf(_Mode, 3)][MixtureVector3]_ScrollDirection("Scroll Vector", Vector) = (0, 1, 0, 0)

		_Multiplier("Multiplier", Range(-100, 100)) = 1
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			HLSLPROGRAM
			#include "Packages/com.alelievr.mixture/Runtime/Shaders/MixtureFixed.hlsl"
			#include "Packages/com.alelievr.mixture/Runtime/Shaders/PerlinNoise.hlsl"
            #pragma vertex CustomRenderTextureVertexShader
			#pragma fragment MixtureFragment
			#pragma target 3.0

            #pragma shader_feature CRT_2D CRT_3D CRT_CUBE
			#pragma shader_feature _ USE_CUSTOM_UV
			#define _TILINGMODE_NONE;

			TEXTURE_X(_UV);
			float _Mode;
			float3 _Direction;
			float _Multiplier;
			float _PointInwards;
			float _StripeCount;
			float _Randomness;
			float _Seed;
			float _Frequency;
			float3 _ScrollDirection;

			float SampleNoise(float3 uv)
			{
#ifdef CRT_2D
				float n = GeneratePerlin2DNoise(uv.xy, _Frequency, 4, 2, 2, _Seed).x;
#else
				float n = GeneratePerlin2DNoise(uv, _Frequency, 4, 2, 2, _Seed).x;
#endif
				return n;
			}

			float4 mixture (v2f_customrendertexture i) : SV_Target
			{
#ifdef USE_CUSTOM_UV
				float3 uv = SAMPLE_X_NEAREST_CLAMP(_UV, i.localTexcoord.xyz, i.direction).xyz;
#else
				float3 uv = GetDefaultUVs(i);
#endif

				float3 direction = 0;
				switch (_Mode)
				{
					default:
					case 0: // Direction
						direction = _Direction;
						break;
					case 1: // Circular:
						uv = uv * 2 - 1;

						// TODO: this doesn't work in 3D
						direction = cross(normalize(uv), float3(0, 0, 1));
						if (_PointInwards > 0)
							direction = lerp(direction, -uv, _PointInwards);
						else
							direction = lerp(direction, uv, -_PointInwards);
						break;
					case 2: // Stripes
						uv *= _StripeCount;

						float r = WhiteNoise(float3(int(uv.x) * 10 + _Seed, 0, 0));
						float t = lerp(0.5, r, _Randomness);
						bool up = frac(uv).x > t;
						float v = 0;
						
						if (up)
							v = WhiteNoise(float3(int(uv.x) * 10 + up * 20 + _Seed, 0, 0));
						else
							v = WhiteNoise(float3(int(uv.x) * 10 - 1 * 20 + _Seed, 0, 0));

						direction = up ? float3(0, v, 0) : float3(0, -v, 0);
						break;
					case 3: // Turbulence
						uv -= _ScrollDirection * _Time.x;
						float d0 = SampleNoise(uv);
						float d1 = SampleNoise(uv + 2);
						float d2 = SampleNoise(uv + 4);
						direction = float3(d0, d1, d2);
						break;
				}

#ifdef CRT_2D
				direction.z = 0;
#endif

				return float4(direction * _Multiplier, 1);
			}
			ENDHLSL
		}
	}
}
