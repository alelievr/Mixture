Shader "Hidden/Mixture/TileWrap"
{	
	Properties
	{
		[InlineTexture]_Texture_2D("Texture", 2D) = "white" {}
		[InlineTexture]_Texture_3D("Texture", 3D) = "white" {}
		[InlineTexture]_Texture_Cube("Texture", Cube) = "white" {}
		[MixtureVector3]_Wrap("UV Wrap", Vector) = (0.2,0.2,0.2,0.0)
	}

	CGINCLUDE

	#include "MixtureFixed.cginc"
	#pragma vertex CustomRenderTextureVertexShader
	#pragma fragment mixture
	#pragma target 3.0

	#pragma shader_feature CRT_2D CRT_3D CRT_CUBE

	TEXTURE_SAMPLER_X(_Texture);

	float4 _Wrap;

	float4 UTiling(v2f_customrendertexture i)
	{
		float mask = smoothstep(1.0 - _Wrap.x, 1.0, i.localTexcoord.x);

		i.localTexcoord.xyz *= (1.0 - _Wrap.xyz * float3(1, 0, 0));
		i.localTexcoord.xyz += _Wrap.xyz * float3(1, 0, 0);

		float4 col = SAMPLE_X(_Texture, i.localTexcoord.xyz, i.direction);
		col = lerp(col, SAMPLE_X(_Texture, i.localTexcoord.xyz - float3(1.0 - _Wrap.x, 0, 0), i.direction), mask);
		return col;
	}

	float4 VTiling(v2f_customrendertexture i)
	{
		float mask = smoothstep(1.0 - _Wrap.y, 1.0, i.localTexcoord.y);

		i.localTexcoord.xyz *= (1.0 - _Wrap.xyz * float3(0, 1, 0));
		i.localTexcoord.xyz += _Wrap.xyz * float3(0, 1, 0);

		float4 col = SAMPLE_SELF_LINEAR_CLAMP(i.localTexcoord.xyz, i.direction);
		col = lerp(col, SAMPLE_SELF_LINEAR_CLAMP(i.localTexcoord.xyz - float3(0, 1.0 - _Wrap.y, 0), i.direction), mask);
		return col;
	}

	float4 WTiling(v2f_customrendertexture i)
	{
		float mask = smoothstep(1.0 - _Wrap.z, 1.0, i.localTexcoord.z);

		i.localTexcoord.xyz *= (1.0 - _Wrap.xyz * float3(0, 0, 1));
		i.localTexcoord.xyz += _Wrap.xyz * float3(0, 0, 1);

		float4 col = SAMPLE_SELF_LINEAR_CLAMP(i.localTexcoord.xyz, i.direction);
		col = lerp(col, SAMPLE_SELF_LINEAR_CLAMP(i.localTexcoord.xyz - float3(0, 0, 1.0 - _Wrap.z), i.direction), mask);
		return col;
	}


	ENDCG

	SubShader
	{
		Tags{ "RenderType" = "Opaque" }
			LOD 100

		Pass
		{
			Name "U Tiling"

			CGPROGRAM
			float4 mixture(v2f_customrendertexture i) : SV_Target
			{
				return UTiling(i);
			}
			ENDCG
		}

		Pass
		{
			Name "V Tiling"

			CGPROGRAM
			float4 mixture(v2f_customrendertexture i) : SV_Target
			{
				return VTiling(i);
			}
			ENDCG
		}

		Pass
		{
			Name "W Tiling"

			CGPROGRAM
			float4 mixture(v2f_customrendertexture i) : SV_Target
			{
				return WTiling(i);
			}
			ENDCG
		}
	}
}
