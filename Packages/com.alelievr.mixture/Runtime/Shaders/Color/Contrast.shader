Shader "Hidden/Mixture/Contrast"
{	
	Properties
	{
		// By default a shader node is supposed to handle all the input texture dimension, we use a prefix to determine which one is used
		[InlineTexture]_Source_2D("Source", 2D) = "white" {}
		[InlineTexture]_Source_3D("Source", 3D) = "white" {}
		[InlineTexture]_Source_Cube("Source", Cube) = "white" {}

		// Other parameters
		[Tooltip(Adjusts the contrast of the image)]_Saturation("Saturation", Range(-1, 1)) = 0
		[Tooltip(Adjusts the luminosity or brightness of the image)]_Luminosity("Luminosity", Range(-1, 1)) = 0
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

			// The list of defines that will be active when processing the node with a certain dimension
            #pragma shader_feature CRT_2D CRT_3D CRT_CUBE

			// This macro will declare a version for each dimention (2D, 3D and Cube)
			TEXTURE_SAMPLER_X(_Source);
			float _Saturation;
			float _Luminosity;

			float zeroToInf(float x)
			{
				x = min(0.999999, x);
				return (1.0 / (-x + 1.0)) - 1.0;
			}

			float4 mixture (v2f_customrendertexture i) : SV_Target
			{
				// The SAMPLE_X macro handles sampling for 2D, 3D and cube textures
				float4 color = SAMPLE_X(_Source, i.localTexcoord.xyz, i.direction);

				float3 hsl = RGBtoHSL(color.rgb);

				float lumFactor = _Luminosity * 0.5 + 0.5;
				float contrastFactor = _Saturation * 0.5 + 0.5;

				contrastFactor = zeroToInf(contrastFactor);
				lumFactor = zeroToInf(lumFactor);

				hsl.z *= lumFactor;
				hsl.y *= contrastFactor;

				color.rgb = HSLtoRGB(hsl);

				return color;
			}
			ENDHLSL
		}
	}
}
