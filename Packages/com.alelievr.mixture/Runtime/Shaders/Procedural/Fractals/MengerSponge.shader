Shader "Hidden/Mixture/MengerSponge"
{	
	Properties
	{
		// By default a shader node is supposed to handle all the input texture dimension, we use a prefix to determine which one is used
		[InlineTexture]_UV_2D("UV", 2D) = "white" {}
		[InlineTexture]_UV_3D("UV", 3D) = "white" {}
		[InlineTexture]_UV_Cube("UV", Cube) = "white" {}

		// Other parameters
		[Enum(Mask, 0, UV, 1)]_Mode("Mode", Float) = 0
		[Int]_Iteration("Iteration", Range(1, 16)) = 4
		_HoleSize("Hole Size", Float) = 3
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
			float _Mode;
			float _HoleSize;

			float4 mengerSponge(float2 uv)
			{
				float sponge = 1.0;
				float startHoleSize = rcp(max(0.0001, _HoleSize));
				float holesize = startHoleSize;
			
				// Check if point is within 1/3 and 2/3 of the unit square then iteratively
				// do the same for smaller sub squares. Exit's early if the size of the holes
				// being checked is smaller than can be rendered/seen. 
				for (int i = 0; i < _Iteration; i++)
				{
					float2 checker = step(-startHoleSize, uv * 2 - 1) * step(uv * 2 - 1, startHoleSize);
					sponge = 1.0 - checker.x * checker.y;

					if (sponge < 0.5)
						break;

					// Subdivide
					uv = frac(uv*3.0);

					holesize *= startHoleSize;
				}

				switch (_Mode)
				{
					default:
					case 0: // Mask
						return sponge;
					case 1: // UV
						return float4(uv, 0, sponge);
				}
			}

			float4 mixture (v2f_customrendertexture i) : SV_Target
			{
#ifdef USE_CUSTOM_UV
				float4 uv = SAMPLE_X_NEAREST_CLAMP(_UV, i.localTexcoord.xyz, i.direction);
#else
				float4 uv = float4(GetDefaultUVs(i), 1);
#endif

				return mengerSponge(uv.xy);
			}
			ENDHLSL
		}
	}
}
