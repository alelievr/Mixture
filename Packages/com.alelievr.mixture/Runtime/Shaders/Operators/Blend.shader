Shader "Hidden/Mixture/Blend"
{	
	Properties
	{
		// Parameters for 2D
		[InlineTexture]_Source_2D("Source", 2D) = "black" {}
		[InlineTexture]_Target_2D("Target", 2D) = "black" {}
		[InlineTexture]_Mask_2D("Mask", 2D) = "white" {}

		// Parameters for 3D
		[InlineTexture]_Source_3D("Source", 3D) = "black" {}
		[InlineTexture]_Target_3D("Target", 3D) = "black" {}
		[InlineTexture]_Mask_3D("Mask", 3D) = "white" {}

		// Parameters for Cubemaps
		[InlineTexture]_Source_Cube("Source", Cube) = "black" {}
		[InlineTexture]_Target_Cube("Target", Cube) = "black" {}
		[InlineTexture]_Mask_Cube("Mask", Cube) = "white" {}

		_Opacity("Opacity", Range(0, 1)) = 0.5

		// Common parameters
		[Enum(Blend, 0, Additive, 1, Multiplicative, 2, Substractive, 3, Min, 4, Max, 5)]_BlendMode("Blend Mode", Float) = 0
		[Enum(PerChannel, 0, R, 1, G, 2, B, 3, A, 4)]_MaskMode("Mask Mode", Float) = 4
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#include "Packages/com.alelievr.mixture/Runtime/Shaders/MixtureFixed.cginc"
            #pragma vertex CustomRenderTextureVertexShader
			#pragma fragment MixtureFragment
			#pragma target 3.0

            #pragma shader_feature CRT_2D CRT_3D CRT_CUBE

			// This macro will declare a version for each dimention (2D, 3D and Cube)
			TEXTURE_SAMPLER_X(_Source);
			TEXTURE_SAMPLER_X(_Target);
			TEXTURE_SAMPLER_X(_Mask);

			float _BlendMode;
			float _MaskMode;
			bool _UseMask;
			float _Opacity;

			float4 mixture (v2f_customrendertexture i) : SV_Target
			{
				float4	source = SAMPLE_X(_Source, i.localTexcoord.xyz, i.direction);
				float4	target = SAMPLE_X(_Target, i.localTexcoord.xyz, i.direction);
				float4	mask = 0;
				
				switch((uint)_MaskMode)
				{
					case 0 : mask = SAMPLE_X(_Mask, i.localTexcoord.xyz, i.direction).rgba; break;
					case 1 : mask = SAMPLE_X(_Mask, i.localTexcoord.xyz, i.direction).rrrr; break;
					case 2 : mask = SAMPLE_X(_Mask, i.localTexcoord.xyz, i.direction).gggg; break;
					case 3 : mask = SAMPLE_X(_Mask, i.localTexcoord.xyz, i.direction).bbbb; break;
					case 4 : mask = SAMPLE_X(_Mask, i.localTexcoord.xyz, i.direction).aaaa; break;
				}

				mask *= _Opacity;

				switch ((uint)_BlendMode)
				{
					default:
					case 0: return lerp(source, target, mask); // Blend
					case 1: return lerp(source, source + target, mask); // Add
					case 2: return lerp(source, source * target, mask); // Mul
					case 3: return lerp(source, source - target, mask); // Sub
					case 4: return lerp(source, min(source, target), mask); // Min
					case 5: return lerp(source, max(source, target), mask); // Max
				}

				// Should not happen but hey...
				return float4(1.0, 0.0, 1.0, 1.0);
			}
			ENDCG
		}
	}
}