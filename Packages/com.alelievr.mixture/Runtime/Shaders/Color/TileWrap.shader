Shader "Hidden/Mixture/TileWrap"
{	
	Properties
	{
		[InlineTexture]_Texture_2D("Texture", 2D) = "white" {}
		[InlineTexture]_Texture_3D("Texture", 3D) = "white" {}
		[InlineTexture]_Texture_Cube("Texture", Cube) = "white" {}
		[Range]_WrapU("U Wrap", Range(0.0,0.5)) = 0.2
		[Range]_WrapV("V Wrap", Range(0.0,0.5)) = 0.2
		[Range]_WrapW("W Wrap", Range(0.0,0.5)) = 0.2
	}

	HLSLINCLUDE

	#include "Packages/com.alelievr.mixture/Runtime/Shaders/MixtureFixed.hlsl"
	#pragma vertex CustomRenderTextureVertexShader
	#pragma fragment MixtureFragment
	#pragma target 3.0

	#pragma shader_feature CRT_2D CRT_3D CRT_CUBE

	TEXTURE_SAMPLER_X(_Texture);

	float _WrapU;
	float _WrapV;
	float _WrapW;

	float4 UTiling(v2f_customrendertexture i)
	{
		float mask = smoothstep(1.0 - _WrapU, 1.0, i.localTexcoord.x);

		i.localTexcoord.xyz *= (1.0 - float3(_WrapU, 0, 0));
		i.localTexcoord.xyz += float3(_WrapU, 0, 0);

		float4 col = SAMPLE_X(_Texture, i.localTexcoord.xyz, i.direction);
		col = lerp(col, SAMPLE_X(_Texture, i.localTexcoord.xyz - float3(1.0 - _WrapU, 0, 0), i.direction), mask);
		return col;
	}

	float4 VTiling(v2f_customrendertexture i)
	{
		float mask = smoothstep(1.0 - _WrapV, 1.0, i.localTexcoord.y);

		i.localTexcoord.xyz *= (1.0 - float3(0, _WrapV, 0));
		i.localTexcoord.xyz += float3(0, _WrapV, 0);

		float4 col = SAMPLE_SELF_LINEAR_CLAMP(i.localTexcoord.xyz, i.direction);
		col = lerp(col, SAMPLE_SELF_LINEAR_CLAMP(i.localTexcoord.xyz - float3(0, 1.0 - _WrapV, 0), i.direction), mask);
		return col;
	}

	float4 WTiling(v2f_customrendertexture i)
	{
		float mask = smoothstep(1.0 - _WrapW, 1.0, i.localTexcoord.z);

		i.localTexcoord.xyz *= (1.0 - float3(0, 0, _WrapW));
		i.localTexcoord.xyz += float3(0, 0, _WrapW);

		float4 col = SAMPLE_SELF_LINEAR_CLAMP(i.localTexcoord.xyz, i.direction);
		col = lerp(col, SAMPLE_SELF_LINEAR_CLAMP(i.localTexcoord.xyz - float3(0, 0, 1.0 - _WrapW), i.direction), mask);
		return col;
	}

	float4 RestoreOffset(v2f_customrendertexture i)
	{
		i.localTexcoord.xyz = frac(i.localTexcoord.xyz -float3(_WrapU/2, _WrapV/2, _WrapW/2));
		return SAMPLE_SELF_LINEAR_CLAMP(i.localTexcoord.xyz, i.direction);
	}

	ENDHLSL

	SubShader
	{
		Tags{ "RenderType" = "Opaque" }
			LOD 100

		Pass
		{
			Name "U Tiling"

			HLSLPROGRAM
			float4 mixture(v2f_customrendertexture i) : SV_Target
			{
				return UTiling(i);
			}
			ENDHLSL
		}

		Pass
		{
			Name "V Tiling"

			HLSLPROGRAM
			float4 mixture(v2f_customrendertexture i) : SV_Target
			{
				return VTiling(i);
			}
			ENDHLSL
		}

		Pass
		{
			Name "W Tiling"

			HLSLPROGRAM
			float4 mixture(v2f_customrendertexture i) : SV_Target
			{
				return WTiling(i);
			}
			ENDHLSL
		}

		Pass
		{
			Name "RestoreOffset"

			HLSLPROGRAM
			float4 mixture(v2f_customrendertexture i) : SV_Target
			{
				return RestoreOffset(i);
			}
			ENDHLSL
		}
	}
}
