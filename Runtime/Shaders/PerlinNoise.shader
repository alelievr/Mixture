Shader "Hidden/Mixture/PerlinNoise"
{	
	Properties
	{
		[InlineTexture(HideInNodeInspector)] _UV_2D("UVs", 2D) = "white" {}
		[InlineTexture(HideInNodeInspector)] _UV_3D("UVs", 3D) = "white" {}
		[InlineTexture(HideInNodeInspector)] _UV_Cube("UVs", Cube) = "white" {}

		_Scale("Scale", Float) = 1
		_Lacunarity("Lacunarity", Float) = 0.5
		_Frequency("Frequency", Float) = 1.2
		_Octaves("Octaves", Int) = 0
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#include "Packages/com.alelievr.mixture/Runtime/Shaders/MixtureFixed.cginc"
			#include "UnityCustomRenderTexture.cginc"
            #pragma vertex CustomRenderTextureVertexShader
			#pragma fragment mixture
			#pragma target 3.0

			// The list of defines that will be active when processing the node with a certain dimension
            #pragma multi_compile CRT_2D CRT_3D CRT_CUBE
			#pragma multi_compile _ USE_CUSTOM_UV

			// This macro will declare a version for each dimention (2D, 3D and Cube)
			TEXTURE_SAMPLER_X(_Source);
			TEXTURE_SAMPLER_X(_UV);
			float _Octaves;

			float4 Interpolation_C2_InterpAndDeriv(float2 x) { return x.xyxy * x.xyxy * (x.xyxy * (x.xyxy * (x.xyxy * float2(6.0f, 0.0f).xxyy + float2(-15.0f, 30.0f).xxyy) + float2(10.0f, -60.0f).xxyy) + float2(0.0f, 30.0f).xxyy); }

			// Generates 2 random numbers for each of the 4 cell corners
			void NoiseHash2D(float2 gridcell, out float4 hash_0, out float4 hash_1)
			{
				float2 kOffset = float2(26.0f, 161.0f);
				float kDomain = 71.0f;
				float2 kLargeFloats = 1.0f / float2(951.135664f, 642.949883f);

				float4 P = float4(gridcell.xy, gridcell.xy + 1.0f);
				P = P - floor(P * (1.0f / kDomain)) * kDomain;
				P += kOffset.xyxy;
				P *= P;
				P = P.xzxz * P.yyww;
				hash_0 = frac(P * kLargeFloats.x);
				hash_1 = frac(P * kLargeFloats.y);
			}

			float samplePerlinNoise2D(float3 coords)
			{
				float2 coordinate = coords.xy;

				// establish our grid cell and unit position
				float2 i = floor(coordinate);
				float4 f_fmin1 = coordinate.xyxy - float4(i, i + 1.0f);

				// calculate the hash
				float4 hash_x, hash_y;
				NoiseHash2D(i, hash_x, hash_y);

				// calculate the gradient results
				float4 grad_x = hash_x - 0.49999f;
				float4 grad_y = hash_y - 0.49999f;
				float4 norm = rsqrt(grad_x * grad_x + grad_y * grad_y);
				grad_x *= norm;
				grad_y *= norm;
				float4 dotval = (grad_x * f_fmin1.xzxz + grad_y * f_fmin1.yyww);

				// convert our data to a more parallel format
				float3 dotval0_grad0 = float3(dotval.x, grad_x.x, grad_y.x);
				float3 dotval1_grad1 = float3(dotval.y, grad_x.y, grad_y.y);
				float3 dotval2_grad2 = float3(dotval.z, grad_x.z, grad_y.z);
				float3 dotval3_grad3 = float3(dotval.w, grad_x.w, grad_y.w);

				// evaluate common constants
				float3 k0_gk0 = dotval1_grad1 - dotval0_grad0;
				float3 k1_gk1 = dotval2_grad2 - dotval1_grad1;
				float3 k2_gk2 = dotval3_grad3 - dotval2_grad2 - k0_gk0;

				// C2 Interpolation
				float4 blend = Interpolation_C2_InterpAndDeriv(f_fmin1.xy);

				// calculate final noise + deriv
				float3 results = dotval0_grad0
					+ blend.x * k0_gk0
					+ blend.y * (k1_gk1 + blend.x * k2_gk2);

				results.yz += blend.zw * (float2(k0_gk0.x, k1_gk1.x) + blend.yx * k2_gk2.xx);

				return results * 1.4142135623730950488016887242097f;  // scale to -1.0 -> 1.0 range  *= 1.0/sqrt(0.5)
			}

			float samplePerlinNoise3D(float3 coords)
			{
				return 0;
			}

			float4 mixture (v2f_customrendertexture i) : SV_Target
			{
				// The SAMPLE_X macro handles sampling for 2D, 3D and cube textures
#ifdef USE_CUSTOM_UV
				float3 uvs = SAMPLE_X(_Source, i.localTexcoord.xyz, i.direction);
#else
				float3 uvs = i.localTexcoord.xyz;
#endif

#ifdef CRT_2D
				return samplePerlinNoise2D(uvs);
#else
				return samplePerlinNoise3D(uvs);
#endif

			}
			ENDCG
		}
	}
}
