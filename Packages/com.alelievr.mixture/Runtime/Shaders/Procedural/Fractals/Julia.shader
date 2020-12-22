Shader "Hidden/Mixture/Julia"
{	
	Properties
	{
		// By default a shader node is supposed to handle all the input texture dimension, we use a prefix to determine which one is used
		[InlineTexture]_UV_2D("UV", 2D) = "white" {}
		[InlineTexture]_UV_3D("UV", 3D) = "white" {}
		[InlineTexture]_UV_Cube("UV", Cube) = "white" {}

		// Other parameters
		[Enum(Distance, 0, UV, 1)]_Mode("Mode", Float) = 0
		[Int]_Iteration("Iteration", Range(1, 256)) = 50
		_Param1("C 1", Range(-1, 1)) = -0.2
		_Param2("C 2", Range(-1, 1)) = 0.7
		_Zoom("Zoom", Range(1, 16)) = 2
		[MixtureVector2]_Position("Position", Vector) = (-0.15, 0.1, 0, 0)
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
			#pragma shader_feature _ USE_CUSTOM_UV

			// This macro will declare a version for each dimention (2D, 3D and Cube)
			TEXTURE_X(_UV);
			int _Iteration;
			float _Zoom;
			float2 _Position;
			float _Mode;
			float _Param1;
			float _Param2;

			float4 dcSqr( float4 a )
			{
				return float4(a.x*a.x - a.y*a.y, 
							2.0*a.x*a.y,
							2.0*(a.x*a.z - a.y*a.w),
							2.0*(a.x*a.w + a.y*a.z));
			}

			// Inspired from: https://www.shadertoy.com/view/XsdGRr
			float4 julia(float2 uv)
			{
				uv = uv * 2 - 1;
				float2 p = float2(_Position + uv / (exp2(_Zoom) / 5.0));

				float2 fractalUVs = 0;
				float3 s = float3(-.5, -.5, 1);

				float2 c = float2(_Param1, _Param2);
				float f = 0.0;
				float2 z = p;
				float d = 1e20;
				for (int i = 0; i < _Iteration; i++)
				{
					if ((dot(z, z) > _Iteration))
						break;

					// fc(z) = zÂ² + c		
					z.xy = float2(z.x*z.x - z.y*z.y, 2.0 * z.x * z.y) + c;
					d = min(d, dot(z, z));

					fractalUVs = s.xy + s.z * z;
					f += 1.0;
				}

				d = 1.0 + log2(d) / 16.0;

				fractalUVs.y *= 4;
				fractalUVs = abs(fractalUVs);
				fractalUVs = fractalUVs.yx;
				fractalUVs = saturate(fractalUVs / 50);

				switch (_Mode)
				{
					default:
					case 0: // Distance
						return d;
					case 1: // UV
						return float4(fractalUVs, 0, d);
				}
			}

			float4 mixture (v2f_customrendertexture i) : SV_Target
			{
#ifdef USE_CUSTOM_UV
				float4 uv = SAMPLE_X_NEAREST_CLAMP(_UV, i.localTexcoord.xyz, i.direction);
#else
				float4 uv = float4(GetDefaultUVs(i), 1);
#endif

				return julia(uv.xy);
			}
			ENDHLSL
		}
	}
}
