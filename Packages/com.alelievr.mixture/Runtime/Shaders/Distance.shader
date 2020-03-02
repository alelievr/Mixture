Shader "Hidden/Mixture/Distance"
{	
	Properties
	{
		// By default a shader node is supposed to handle all the input texture dimension, we use a prefix to determine which one is used
		[InlineTexture]_Source_2D("Source", 2D) = "white" {}
		[InlineTexture]_Source_3D("Source", 3D) = "white" {}
		[InlineTexture]_Source_Cube("Source", Cube) = "white" {}

		// Other parameters
		[Range]_Threshold("Threshold", Range(0, 1)) = 0.5
		[Range]_Radius("Radius", Range(0, 32)) = 4
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100
	
		CGINCLUDE
		#include "Packages/com.alelievr.mixture/Runtime/Shaders/MixtureFixed.cginc"
		#pragma target 3.0
		#pragma vertex CustomRenderTextureVertexShader

		// The list of defines that will be active when processing the node with a certain dimension
		#pragma shader_feature CRT_2D CRT_3D CRT_CUBE

		// This macro will declare a version for each dimention (2D, 3D and Cube)
		TEXTURE_X(_Source);
		float _Threshold;
		float _Radius;

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

				if (all(input.rgb > _Threshold))
					return float4(crt.localTexcoord.xyz, 0);
				else
					return float4(-1, -1, -1, -1); // mark UV as invalid in this case
			}

			ENDCG
		}

		Pass
		{
			CGPROGRAM
			#define STEP_LENGTH 0
			TEXTURE_X(_UVMap);
			#include "Packages/com.alelievr.mixture/Runtime/Shaders/Distance.hlsl"
			#pragma fragment MixtureFragment
			ENDCG
		}

		Pass
		{
			CGPROGRAM
			#define STEP_LENGTH 1
			TEXTURE_X(_UVMap);
			#include "Packages/com.alelievr.mixture/Runtime/Shaders/Distance.hlsl"
			#pragma fragment MixtureFragment
			ENDCG
		}

		Pass
		{
			CGPROGRAM
			#define STEP_LENGTH 2
			TEXTURE_X(_UVMap);
			#include "Packages/com.alelievr.mixture/Runtime/Shaders/Distance.hlsl"
			#pragma fragment MixtureFragment
			ENDCG
		}

		Pass
		{
			CGPROGRAM
			#define STEP_LENGTH 3
			TEXTURE_X(_UVMap);
			#include "Packages/com.alelievr.mixture/Runtime/Shaders/Distance.hlsl"
			#pragma fragment MixtureFragment
			ENDCG
		}

		Pass
		{
			CGPROGRAM
			#define STEP_LENGTH 4
			TEXTURE_X(_UVMap);
			#include "Packages/com.alelievr.mixture/Runtime/Shaders/Distance.hlsl"
			#pragma fragment MixtureFragment
			ENDCG
		}

		Pass
		{
			CGPROGRAM
			#define STEP_LENGTH 5
			TEXTURE_X(_UVMap);
			#include "Packages/com.alelievr.mixture/Runtime/Shaders/Distance.hlsl"
			#pragma fragment MixtureFragment
			ENDCG
		}

		Pass
		{
			CGPROGRAM
			#define STEP_LENGTH 6
			TEXTURE_X(_UVMap);
			#include "Packages/com.alelievr.mixture/Runtime/Shaders/Distance.hlsl"
			#pragma fragment MixtureFragment
			ENDCG
		}

		Pass
		{
			CGPROGRAM
			#define STEP_LENGTH 7
			TEXTURE_X(_UVMap);
			#include "Packages/com.alelievr.mixture/Runtime/Shaders/Distance.hlsl"
			#pragma fragment MixtureFragment
			ENDCG
		}

		Pass
		{
			CGPROGRAM
			#define STEP_LENGTH 8
			TEXTURE_X(_UVMap);
			#include "Packages/com.alelievr.mixture/Runtime/Shaders/Distance.hlsl"
			#pragma fragment MixtureFragment
			ENDCG
		}

		Pass
		{
			CGPROGRAM
			#define STEP_LENGTH 9
			TEXTURE_X(_UVMap);
			#include "Packages/com.alelievr.mixture/Runtime/Shaders/Distance.hlsl"
			#pragma fragment MixtureFragment
			ENDCG
		}

		Pass
		{
			CGPROGRAM
			#define STEP_LENGTH 10
			TEXTURE_X(_UVMap);
			#include "Packages/com.alelievr.mixture/Runtime/Shaders/Distance.hlsl"
			#pragma fragment MixtureFragment
			ENDCG
		}

		Pass
		{
			CGPROGRAM
			#define STEP_LENGTH 11
			TEXTURE_X(_UVMap);
			#include "Packages/com.alelievr.mixture/Runtime/Shaders/Distance.hlsl"
			#pragma fragment MixtureFragment
			ENDCG
		}

		Pass
		{
			CGPROGRAM
			#define STEP_LENGTH 12
			TEXTURE_X(_UVMap);
			#include "Packages/com.alelievr.mixture/Runtime/Shaders/Distance.hlsl"
			#pragma fragment MixtureFragment
			ENDCG
		}

		Pass
		{
			CGPROGRAM
			#define STEP_LENGTH 13
			TEXTURE_X(_UVMap);
			#include "Packages/com.alelievr.mixture/Runtime/Shaders/Distance.hlsl"
			#pragma fragment MixtureFragment
			ENDCG
		}

	}
}
