Shader "Hidden/Mixture/GradientMatte"
{	
	Properties
	{
		[Enum(Linear, 0, Exponential, 1, Radial, 2 ,Circular, 3)]_Mode("Gradient Type", Float) = 0
		[VisibleIf(_Mode, 0, 1)][Enum(Up, 0, Down, 1, Right, 2, Left, 3, Forward, 4, Back, 5)]_Direction("Direction", Float) = 0
		_Falloff("Falloff", Float) = 1
		[HDR]_Color1("Color 1", Color) = (0.0,0.0,0.0,0.0)
		[HDR]_Color2("Color 2", Color) = (1.0,1.0,1.0,1.0)
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
	
			float _Mode;
			float _Direction;
			float _Falloff;
			float4 _Color1;
			float4 _Color2;

			float4 mixture (v2f_customrendertexture IN) : SV_Target
			{
				float3 uv = IN.localTexcoord.xyz;
				float gradient = 0.0f;

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
				}

				gradient = pow(abs(gradient), _Falloff);

				return lerp(_Color1, _Color2, saturate(gradient));
			}
			ENDCG
		}
	}
}
