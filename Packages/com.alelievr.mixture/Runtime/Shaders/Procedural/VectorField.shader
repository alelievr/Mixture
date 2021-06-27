Shader "Hidden/Mixture/VectorField"
{	
	Properties
	{
		[InlineTexture]_UV_2D("UV", 2D) = "white" {}
		[InlineTexture]_UV_3D("UV", 3D) = "white" {}
		[InlineTexture]_UV_Cube("UV", Cube) = "white" {}

		[Enum(Direction, 0, Circular, 1)]_Mode("Mode", Float) = 0

		[VisibleIf(_Mode, 0)][MixtureVector3]_Direction("Direction", Vector) = (1, 0, 0, 0)

		[VisibleIf(_Mode, 1)]_PointInwards("Point Inwards", Range(-1, 1)) = 0.2

		_Multiplier("Multiplier", Range(-10, 10)) = 1
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
			#pragma shader_feature _ USE_CUSTOM_UV

			TEXTURE_X(_UV);
			float _Mode;
			float3 _Direction;
			float _Multiplier;
			float _PointInwards;

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
				}

				return float4(direction * _Multiplier, 1);
			}
			ENDHLSL
		}
	}
}
