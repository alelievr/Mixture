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

		[Tooltip(Opacity of the Blend, 0 means that only Source is visible and 1 that only Target is visible)]_Opacity("Opacity", Range(0, 1)) = 0.5

		// Common parameters
		[MixtureBlend]_BlendMode("Blend Mode", Float) = 0
		[Tooltip(Select which channel is used to sample the mask value)][Enum(PerChannel, 0, R, 1, G, 2, B, 3, A, 4)]_MaskMode("Mask Mode", Float) = 4
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

				float4 tmp, result1, result2, zeroOrOne;
				switch ((uint)_BlendMode)
				{
					default:
					case 0: // Normal
						return lerp(source, target, mask);
					case 1: // Min 
						return lerp(source, min(source, target), mask);
					case 2: // Max
						return lerp(source, max(source, target), mask);
					case 3: // Burn
					    tmp =  1.0 - (1.0 - target)/source;
						return lerp(source, tmp, mask);
					case 4: // Darken
						tmp = min(target, source);
						return lerp(source, tmp, mask);
					case 5: // Difference
					    tmp = abs(target - source);
    					return lerp(source, tmp, mask);
					case 6: // Dodge
					    tmp = source / (1.0 - target);
    					return lerp(source, tmp, mask);
					case 7: // Divide
					    tmp = source / (target + 0.000000000001);
    					return lerp(source, tmp, mask);
					case 8: // Exclusion
					    tmp = target + source - (2.0 * target * source);
    					return lerp(source, tmp, mask);
					case 9: // HardLight
					    result1 = 1.0 - 2.0 * (1.0 - source) * (1.0 - target);
						result2 = 2.0 * source * target;
						zeroOrOne = step(target, 0.5);
						tmp = result2 * zeroOrOne + (1 - zeroOrOne) * result1;
    					return lerp(source, tmp, mask);
					case 10: // HardMix
					    tmp = step(1 - source, target);
    					return lerp(source, tmp, mask);
					case 11: // Lighten
					    tmp = max(target, source);
    					return lerp(source, tmp, mask);
					case 12: // LinearBurn
					    tmp = source + target - 1.0;
    					return lerp(source, tmp, mask);
					case 13: // LinearDodge
					    tmp = source + target;
    					return lerp(source, tmp, mask);
					case 14: // LinearLight
					    tmp = target < 0.5 ? max(source + (2 * target) - 1, 0) : min(source + 2 * (target - 0.5), 1);
    					return lerp(source, tmp, mask);
					case 15: // LinearLightAddSub
					    tmp = target + 2.0 * source - 1.0;
    					return lerp(source, tmp, mask);
					case 16: // Multiply
					    tmp = source * target;
    					return lerp(source, tmp, mask);
					case 17: // Negation
					    tmp = 1.0 - abs(1.0 - target - source);
    					return lerp(source, tmp, mask);
					case 18: // Overlay
					    result1 = 1.0 - 2.0 * (1.0 - source) * (1.0 - target);
						result2 = 2.0 * source * target;
						zeroOrOne = step(source, 0.5);
						tmp = result2 * zeroOrOne + (1 - zeroOrOne) * result1;
						return lerp(source, tmp, mask);
					case 19: // PinLight
					    float4 check = step (0.5, target);
						result1 = check * max(2.0 * (source - 0.5), target);
						tmp = result1 + (1.0 - check) * min(2.0 * source, target);
    					return lerp(source, tmp, mask);
					case 20: // Screen
					    tmp = 1.0 - (1.0 - target) * (1.0 - source);
						return lerp(source, tmp, mask);
					case 21: // SoftLight
					    result1 = 2.0 * source * target + source * source * (1.0 - 2.0 * target);
						result2 = sqrt(source) * (2.0 * target - 1.0) + 2.0 * source * (1.0 - target);
						zeroOrOne = step(0.5, target);
						tmp = result2 * zeroOrOne + (1 - zeroOrOne) * result1;
						return lerp(source, tmp, mask);
					case 22: // Subtract
					    tmp = source - target;
						return lerp(source, tmp, mask);
					case 23: // VividLight
					    result1 = 1.0 - (1.0 - target) / (2.0 * source);
						result2 = target / (2.0 * (1.0 - source));
						zeroOrOne = step(0.5, source);
						tmp = result2 * zeroOrOne + (1 - zeroOrOne) * result1;
						return lerp(source, tmp, mask);
				}
			}
			ENDHLSL
		}
	}
}
