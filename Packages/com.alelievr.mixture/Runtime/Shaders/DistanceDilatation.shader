Shader "Hidden/Mixture/DistanceDilatation"
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

	CGINCLUDE

	#include "Packages/com.alelievr.mixture/Runtime/Shaders/MixtureFixed.cginc"
	#pragma vertex CustomRenderTextureVertexShader
	#pragma fragment MixtureFragment
	#pragma target 4.5
	#pragma require randomwrite

	// Distance by dilatation does not work with cubemaps
	#pragma shader_feature CRT_2D CRT_3D

	// This macro will declare a version for each dimention (2D, 3D and Cube)
	TEXTURE_X(_Source);
	float _Threshold;
	float _Radius;

	// RW_TEXTURE_X(FLOAT_X, _DilatationBuffer);

	#define NON_DILATED_PIXEL float4(-1, -1, -1, 1)

	bool IsPixelAboveThreshold(float4 color)
	{
		return all(color.rgb > _Threshold);
	}

	bool IsPixelDilated(float4 color)
	{
#ifdef CRT_2D
		return all(color.rg > 0);
#else
		return all(color.rgb > 0);
#endif
	}

	static int DilatationRadius = 128;

	float4 DilateDirection(float3 uv, float3 direction)
	{
		float4 input = LOAD_SELF(uv, float3(0, 0, 0));
		float4 color = input;

		if (IsPixelDilated(input))
			return color;

		float minLength = 1e20;
		color = NON_DILATED_PIXEL;
		int k = 0;
		for (int x = -DilatationRadius; x <= DilatationRadius; x++)
		{
			if (x == 0)
				continue;

			float l = x / DilatationRadius;
			float3 neighbourUV = uv + (direction * float(x)) / float3(_CustomRenderTextureWidth, _CustomRenderTextureHeight, _CustomRenderTextureDepth);
			float4 neighbour = LOAD_SELF(neighbourUV, crt.direction);

			if (IsPixelDilated(neighbour))
			{
				if (l < minLength)
				{
					minLength = l;
					color = neighbour;
				}
			}
		}

		return color;
	}

	ENDCG

	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		pass
		{
			// This first pass will write all pixels that need to be dilated
			Name "InitDilatationBuffer"

			CGPROGRAM

			float4 mixture (v2f_customrendertexture crt) : SV_Target
			{
				float4 input = LOAD_X(_Source, crt.localTexcoord.xyz, crt.direction);

				if (IsPixelAboveThreshold(input))
					return float4(crt.localTexcoord.xyz, 1);
				else
					return NON_DILATED_PIXEL;
			}

			ENDCG
		}

		Pass
		{
			Name "Dilatation X"

			CGPROGRAM
			
			float4 mixture (v2f_customrendertexture crt) : SV_Target
			{
				return DilateDirection(crt.localTexcoord.xyz, float3(1, 0, 0));
			}

			ENDCG
		}

		Pass
		{
			Name "Dilatation Y"

			CGPROGRAM
			
			float4 mixture (v2f_customrendertexture crt) : SV_Target
			{
				return DilateDirection(crt.localTexcoord.xyz, float3(0, 1, 0));
			}

			ENDCG
		}

		Pass
		{
			Name "Dilatation Z"

			CGPROGRAM
			
			float4 mixture (v2f_customrendertexture crt) : SV_Target
			{
				return DilateDirection(crt.localTexcoord.xyz, float3(0, 0, 1));
			}

			ENDCG
		}
	}
}
