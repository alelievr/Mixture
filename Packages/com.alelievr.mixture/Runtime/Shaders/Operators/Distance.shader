Shader "Hidden/Mixture/Distance"
{	
	Properties
	{
		// By default a shader node is supposed to handle all the input texture dimension, we use a prefix to determine which one is used
		[InlineTexture]_Source_2D("Source", 2D) = "white" {}
		[InlineTexture]_Source_3D("Source", 3D) = "white" {}
		[InlineTexture]_Source_Cube("Source", Cube) = "white" {}

		// Other parameters
		[Tooltip(Threshold value used to determine if we flood fill the pixel. The Luminance of the input textureis used to test against this threshold]
		[Range]_Threshold("Threshold", Range(0, 1)) = 0.5
		[Range]_Radius("Radius", Range(0, 1)) = 0.2
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100
	
		CGINCLUDE
		#include "Packages/com.alelievr.mixture/Runtime/Shaders/MixtureFixed.cginc"
	#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
		#pragma target 3.0
		#pragma vertex CustomRenderTextureVertexShader
		#pragma enable_d3d11_debug_symbols

		// The list of defines that will be active when processing the node with a certain dimension
		#pragma shader_feature CRT_2D CRT_3D CRT_CUBE

		// This macro will declare a version for each dimention (2D, 3D and Cube)
		TEXTURE_X(_Source);
		TEXTURE_X(_UVMap);
		float _Threshold;
		float _SourceMipCount;
		float _Radius;

		bool PassThreshold(float3 color)
		{
			return Luminance(color) > _Threshold;
		}

		ENDCG

		Pass
		{
			Name "Fill UV map"
			CGPROGRAM
			#pragma fragment FillUVMap

			float4 FillUVMap(v2f_customrendertexture crt) : SV_Target
			{
				FIX_CUBEMAP_DIRECTION(crt);

				float4 input = LOAD_X(_Source, crt.localTexcoord.xyz, crt.direction);

				if (PassThreshold(input.rgb))
					return float4(crt.localTexcoord.xyz, 1);
				else
					return float4(0, 0, 0, 0); // mark UV as invalid with w = 0
			}

			ENDCG
		}

		Pass
		{
			Name "Final UV to color"
			CGPROGRAM
			#pragma fragment Final

			float4 Final(v2f_customrendertexture crt) : SV_Target
			{
				FIX_CUBEMAP_DIRECTION(crt);

				float4 input = LOAD_X(_Source, crt.localTexcoord.xyz, crt.direction);
				float3 defaultUV = GetDefaultUVs(crt);
				float3 uv = defaultUV;
				float fadeFactor = 1;

				if (!PassThreshold(input.rgb))
				{
					float4 uvValue = SAMPLE_SELF_SAMPLER(s_point_repeat_sampler, crt.localTexcoord.xyz, crt.direction);

					// Only assign UV when they are valid
					if (uvValue.w > 0.5)
						uv = uvValue.xyz;
				}

				float dist = length(frac(uv - crt.localTexcoord.xyz + 0.5) - 0.5);

				if (all(uv == defaultUV))
					dist = 0;

				if (dist >= _Radius)
					uv = defaultUV;
				else
					fadeFactor = 1 - (dist / max(_Radius, 0.000001));

				float4 finalColor = SAMPLE_X_SAMPLER(_Source, s_point_repeat_sampler, uv, uv);

				// merge input alpha and dilated alpha to avoid artifacts:
				finalColor.a *= max(fadeFactor, input.a);

				return finalColor;
			}

			ENDCG
		}

		Pass
		{
			CGPROGRAM
			#define JUMP_INDEX 0
			#include "Packages/com.alelievr.mixture/Runtime/Shaders/Distance.hlsl"
			#pragma fragment MixtureFragment
			ENDCG
		}

		Pass
		{
			CGPROGRAM
			#define JUMP_INDEX 1
			#include "Packages/com.alelievr.mixture/Runtime/Shaders/Distance.hlsl"
			#pragma fragment MixtureFragment
			ENDCG
		}

		Pass
		{
			CGPROGRAM
			#define JUMP_INDEX 2
			#include "Packages/com.alelievr.mixture/Runtime/Shaders/Distance.hlsl"
			#pragma fragment MixtureFragment
			ENDCG
		}

		Pass
		{
			CGPROGRAM
			#define JUMP_INDEX 3
			#include "Packages/com.alelievr.mixture/Runtime/Shaders/Distance.hlsl"
			#pragma fragment MixtureFragment
			ENDCG
		}

		Pass
		{
			CGPROGRAM
			#define JUMP_INDEX 4
			#include "Packages/com.alelievr.mixture/Runtime/Shaders/Distance.hlsl"
			#pragma fragment MixtureFragment
			ENDCG
		}

		Pass
		{
			CGPROGRAM
			#define JUMP_INDEX 5
			#include "Packages/com.alelievr.mixture/Runtime/Shaders/Distance.hlsl"
			#pragma fragment MixtureFragment
			ENDCG
		}

		Pass
		{
			CGPROGRAM
			#define JUMP_INDEX 6
			#include "Packages/com.alelievr.mixture/Runtime/Shaders/Distance.hlsl"
			#pragma fragment MixtureFragment
			ENDCG
		}

		Pass
		{
			CGPROGRAM
			#define JUMP_INDEX 7
			#include "Packages/com.alelievr.mixture/Runtime/Shaders/Distance.hlsl"
			#pragma fragment MixtureFragment
			ENDCG
		}

		Pass
		{
			CGPROGRAM
			#define JUMP_INDEX 8
			#include "Packages/com.alelievr.mixture/Runtime/Shaders/Distance.hlsl"
			#pragma fragment MixtureFragment
			ENDCG
		}

		Pass
		{
			CGPROGRAM
			#define JUMP_INDEX 9
			#include "Packages/com.alelievr.mixture/Runtime/Shaders/Distance.hlsl"
			#pragma fragment MixtureFragment
			ENDCG
		}

		Pass
		{
			CGPROGRAM
			#define JUMP_INDEX 10
			#include "Packages/com.alelievr.mixture/Runtime/Shaders/Distance.hlsl"
			#pragma fragment MixtureFragment
			ENDCG
		}

		Pass
		{
			CGPROGRAM
			#define JUMP_INDEX 11
			#include "Packages/com.alelievr.mixture/Runtime/Shaders/Distance.hlsl"
			#pragma fragment MixtureFragment
			ENDCG
		}

		Pass
		{
			CGPROGRAM
			#define JUMP_INDEX 12
			#include "Packages/com.alelievr.mixture/Runtime/Shaders/Distance.hlsl"
			#pragma fragment MixtureFragment
			ENDCG
		}

		Pass
		{
			CGPROGRAM
			#define JUMP_INDEX 13
			#include "Packages/com.alelievr.mixture/Runtime/Shaders/Distance.hlsl"
			#pragma fragment MixtureFragment
			ENDCG
		}

	}
}
