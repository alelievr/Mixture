Shader "Hidden/Mixture/GradientMatte"
{	
	Properties
	{
		[Tooltip(Outer color)][HDR]_Color1("Color 1", Color) = (0.0,0.0,0.0,0.0)
		[Tooltip(Inner color)][HDR]_Color2("Color 2", Color) = (1.0,1.0,1.0,1.0)
		[Tooltip(Style of the gradient)][Enum(Linear, 0, Exponential, 1, Radial, 2, Circular, 3, Square, 4, Spiral, 5)]_Mode("Gradient Type", Float) = 0
		[Tooltip(Direction of the gradient, only visible on the Linear and exponential gradients)][VisibleIf(_Mode, 0, 1)][Enum(Up, 0, Down, 1, Right, 2, Left, 3, Forward, 4, Back, 5)]_Direction("Direction", Float) = 0
		[Tooltip(Turn count of the spiral)][VisibleIf(_Mode, 5)]_SpiralTurnCount("Turn Count", Float) = 1
		[Tooltip(Turn count of the spiral)][VisibleIf(_Mode, 5)]_SpiralBranchCount("Branch Count", Int) = 1
		[Tooltip(Exponential falloff of the gradient)]_Falloff("Falloff", Float) = 1
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
	
			float _Mode;
			float _Direction;
			float _Falloff;
			float4 _Color1;
			float4 _Color2;
			float _SpiralTurnCount;
			int _SpiralBranchCount;

			float4 mixture (v2f_customrendertexture IN) : SV_Target
			{
				float3 uv = IN.localTexcoord.xyz;
				float gradient = 0.0f;

// For cubemaps, we don't want the z coordinate to mess with the gradients
#ifdef CRT_CUBE
				uv.z = 0.5;
#endif

				switch ((uint)_Mode)
				{
					case 0:
					case 1:
						switch (_Direction)
						{
							default:
							case 0: gradient = uv.y; break;
							case 1: gradient = 1 - uv.y; break;
							case 2: gradient = uv.x; break;
							case 3: gradient = 1 - uv.x; break;
							case 4: gradient = uv.z; break;
							case 5: gradient = 1 - uv.z; break;
						}
						break;
					case 2: uv -= 0.5; gradient = pow(saturate(1.0 - (dot(uv, uv) * 4.0)), 2.0); break;
					case 3: uv -= 0.5; gradient = saturate((atan2(uv.y, uv.x) / 6.283185307179586476924) + 0.5); break;
					case 4: uv -= 0.5; uv = abs(uv); gradient = max(max(uv.x, uv.y), uv.z) * 2; break;
					case 5:
						uv -= 0.5;
						float angle = atan2(uv.y, uv.x);
						gradient = sin(length(uv) * (_SpiralTurnCount * 2 * PI) + angle / 2 * _SpiralBranchCount);
						break;
				}

				gradient = pow(abs(gradient), _Falloff);

				return lerp(_Color1, _Color2, saturate(gradient));
			}
			ENDHLSL
		}
	}
}
