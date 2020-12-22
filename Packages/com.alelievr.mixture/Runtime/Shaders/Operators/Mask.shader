Shader "Hidden/Mixture/Mask"
{	
	Properties
	{
		[InlineTexture]_Target_2D("Target", 2D) = "white" {}
		[InlineTexture]_Target_3D("Target", 3D) = "white" {}
		[InlineTexture]_Target_Cube("Target", Cube) = "white" {}

		[InlineTexture]_Source_2D("Input", 2D) = "white" {}
		[InlineTexture]_Source_3D("Input", 3D) = "white" {}
		[InlineTexture]_Source_Cube("Input", Cube) = "white" {}

		[MixtureChannel]_Mask("Alpha", Float) = 3
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

			TEXTURE_SAMPLER_X(_Target);
			TEXTURE_SAMPLER_X(_Source);

			float _Mask;

			float ChannelMask(float4 sourceValue, uint mode)
			{
				switch (mode)
				{
				case 0: return sourceValue.x;
				case 1: return sourceValue.y;
				case 2: return sourceValue.z;
				default:
				case 3: return sourceValue.w;
				}
				return 0;
			}

			float4 mixture (v2f_customrendertexture i) : SV_Target
			{
				float4 source = SAMPLE_X(_Source, i.localTexcoord.xyz, i.direction);
				float4 target = SAMPLE_X(_Target, i.localTexcoord.xyz, i.direction);

				float a = ChannelMask(source, _Mask);
				return float4(target.r, target.g, target.b, a);
			}
			ENDHLSL
		}
	}
}
