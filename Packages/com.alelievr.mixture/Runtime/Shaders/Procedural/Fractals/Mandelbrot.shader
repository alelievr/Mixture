Shader "Hidden/Mixture/Mandelbrot"
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
		_Zoom("Zoom", Range(1, 16)) = 1
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

			float4 dcSqr( float4 a )
			{
				return float4(a.x*a.x - a.y*a.y, 
							2.0*a.x*a.y,
							2.0*(a.x*a.z - a.y*a.w),
							2.0*(a.x*a.w + a.y*a.z));
			}

			float4 mandelbrot(float2 uv)
			{
				uv = uv * 2 - 1;
				float4 c = float4(_Position + uv / (exp2(_Zoom) / 5.0), 10.0, 0.0);

				float m2 = 0.0;
				float co = 0.0;
				
				float4 z = float4( 0.0, 0.0, 0.0, 0.0 );
				
				float2 fractalUVs = 0;
				float3 s = float3(-.5, -.5, 1);

				for (int i = 0; i < _Iteration; i++)
				{
					if (m2 > 1024.0)
						break;

					// Z -> Z² + c		
					z = dcSqr(z) + c;
					
					m2 = dot(z.xy, z.xy);

					fractalUVs = s.xy + s.z * z.xy;

					co += 1.0;
				}

				// distance	
				// d(c) = |Z|·log|Z|/|Z'|
				float d = 0.0;
				if (co < 256.0)
					d = sqrt(dot(z.xy, z.xy) / dot(z.zw, z.zw)) * log(dot(z.xy, z.xy));

				// do some soft coloring based on distance
				d = clamp(4.0 * d, 0.0, 1.0);
				d = pow(d, 0.25 );

				fractalUVs.y *= 4;
				fractalUVs = abs(fractalUVs);
				fractalUVs = fractalUVs.yx;
				fractalUVs = saturate(fractalUVs / 500);

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

				return mandelbrot(uv.xy);
			}
			ENDHLSL
		}
	}
}
