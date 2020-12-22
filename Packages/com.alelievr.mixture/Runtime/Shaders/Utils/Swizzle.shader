Shader "Hidden/Mixture/Swizzle"
{	
	Properties
	{
		[InlineTexture]_Source_2D("Input", 2D) = "white" {}
		[InlineTexture]_Source_3D("Input", 3D) = "white" {}
		[InlineTexture]_Source_Cube("Input", Cube) = "white" {}

		[MixtureSwizzle]_RMode("Output Red", Float) = 0
		[MixtureSwizzle]_GMode("Output Green", Float) = 1
		[MixtureSwizzle]_BMode("Output Blue", Float) = 2
		[MixtureSwizzle]_AMode("Output Alpha", Float) = 3

		[HDR]_Custom("Custom", Color) = (1.0,1.0,1.0,1.0)
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

			TEXTURE_SAMPLER_X(_Source);

			float _RMode;
			float _GMode;
			float _BMode;
			float _AMode;
			float4 _Custom;

			float4 mixture (v2f_customrendertexture i) : SV_Target
			{
				float4 source = SAMPLE_X(_Source, i.localTexcoord.xyz, i.direction);
				float r = Swizzle(source, _RMode, _Custom.r);
				float g = Swizzle(source, _GMode, _Custom.g);
				float b = Swizzle(source, _BMode, _Custom.b);
				float a = Swizzle(source, _AMode, _Custom.a);
				return float4(r, g, b, a);
			}
			ENDHLSL
		}
	}
}
