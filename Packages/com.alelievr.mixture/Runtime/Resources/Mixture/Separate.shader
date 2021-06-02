Shader "Hidden/Mixture/Separate"
{	
	Properties
	{
		// By default a shader node is supposed to handle all the input texture dimension, we use a prefix to determine which one is used
		[InlineTexture]_Source_2D("Source", 2D) = "white" {}
		[InlineTexture]_Source_3D("Source", 3D) = "white" {}
		[InlineTexture]_Source_Cube("Source", Cube) = "white" {}

		// Other parameters
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
            #pragma enable_d3d11_debug_symbols

			// The list of defines that will be active when processing the node with a certain dimension
            #pragma shader_feature CRT_2D CRT_3D CRT_CUBE

			// This macro will declare a version for each dimention (2D, 3D and Cube)
			TEXTURE_SAMPLER_X(_Source);
			uint _Component;
			float4 _NeutralColor;
			float _Mode;

			float4 SetComponent(float4 color, float component)
			{
				switch (_Component)
				{
					default:
					case 0:
						return float4(component, color.gba);
					case 1:
						return float4(color.r, component, color.ba);
					case 2:
						return float4(color.rg, component, color.a);
					case 3:
						return float4(color.rgb, component);
				}
			}

			float4 mixture (v2f_customrendertexture i) : SV_Target
			{
				// The SAMPLE_X macro handles sampling for 2D, 3D and cube textures
				float c = SAMPLE_X(_Source, i.localTexcoord.xyz, i.direction)[_Component];

				switch (_Mode)
				{
					default:
					case 0: // RGBA
						return SetComponent(_NeutralColor, c);
					case 1: // R
						return c;
				}
			}

			ENDHLSL
		}
	}
}
