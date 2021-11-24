Shader "Hidden/Mixture/DirectionalBlur"
{	
	Properties
	{
		// By default a shader node is supposed to handle all the input texture dimension, we use a prefix to determine which one is used
		[InlineTexture]_Source_2D("Source", 2D) = "white" {}
		[InlineTexture]_Source_3D("Source", 3D) = "white" {}
		[InlineTexture]_Source_Cube("Source", Cube) = "white" {}

		[Tooltip(Radius of the blur kernel, you have a good quality under 64 pixel of radius)]_Radius("Radius", Float) = 0
		[Tooltip(Direction vector, note that it does not have to be normalized)][MixtureVector2]_Direction("Direction", Vector) = (0.707, 0.707, 0, 0)
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

			static float gaussianWeights[32] = {0.03740084,
				0.03723684,
				0.03674915,
				0.03595048,
				0.03486142,
				0.03350953,
				0.03192822,
				0.03015531,
				0.02823164,
				0.02619939,
				0.02410068,
				0.02197609,
				0.01986344,
				0.01779678,
				0.01580561,
				0.01391439,
				0.01214227,
				0.01050313,
				0.009005766,
				0.007654299,
				0.006448714,
				0.005385472,
				0.004458177,
				0.003658254,
				0.002975593,
				0.002399142,
				0.001917438,
				0.001519042,
				0.001192892,
				0.0009285718,
				0.0007164943,
				0.0005480157,
			};

			// The list of defines that will be active when processing the node with a certain dimension
            #pragma shader_feature CRT_2D CRT_3D CRT_CUBE

			// This macro will declare a version for each dimention (2D, 3D and Cube)
			TEXTURE_X(_Source);
			SAMPLER_X(sampler_Source);
			float _Radius;
			float2 _Direction;

			float4 mixture (v2f_customrendertexture i) : SV_Target
			{
				float4 color = SAMPLE_X_SAMPLER(_Source, sampler_Source, float3(i.localTexcoord.xy, 0), i.direction);

				if (_Radius == 0)
					return color;

				color *= gaussianWeights[0];

				for (int j = 1; j < 32; j++)
				{
					float2 uvOffset = _Direction * _Radius / _CustomRenderTextureWidth * j / 32;

					color += SAMPLE_X_SAMPLER(_Source, sampler_Source, float3(i.localTexcoord.xy + uvOffset, 0), i.direction) * gaussianWeights[j];
					color += SAMPLE_X_SAMPLER(_Source, sampler_Source, float3(i.localTexcoord.xy - uvOffset, 0), i.direction) * gaussianWeights[j];
				}

				return color;
			}
			ENDHLSL
		}
	}
}
