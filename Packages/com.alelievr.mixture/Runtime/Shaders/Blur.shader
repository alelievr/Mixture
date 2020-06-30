Shader "Hidden/Mixture/Blur"
{	
	Properties
	{
		// By default a shader node is supposed to handle all the input texture dimension, we use a prefix to determine which one is used
		[Tooltip(Source Texture)][InlineTexture]_Source_2D("Source", 2D) = "white" {}
		[Tooltip(Source Texture)][InlineTexture]_Source_3D("Source", 3D) = "white" {}
		[Tooltip(Source Texture)][InlineTexture]_Source_Cube("Source", Cube) = "white" {}

		[Tooltip(Blur radius in pixels)]_Radius("Radius", Float) = 0
		// Other parameters
	}

	CGINCLUDE
	
	#include "Packages/com.alelievr.mixture/Runtime/Shaders/MixtureFixed.cginc"

	#pragma target 3.0
	// The list of defines that will be active when processing the node with a certain dimension
	#pragma shader_feature CRT_2D CRT_3D CRT_CUBE
	#pragma vertex CustomRenderTextureVertexShader
	#pragma fragment MixtureFragment

	#define SAMPLE_COUNT 32
	static float gaussianWeights[SAMPLE_COUNT] = {0.03740084,
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
	
	TEXTURE_X(_Source);
	float _Radius;

	float4 GaussianBlur(v2f_customrendertexture i, float3 direction, bool sampleSelf)
	{
		float4 color;
		
		if (sampleSelf)
			color = SAMPLE_SELF_LINEAR_CLAMP(float3(i.localTexcoord.xy, 0), i.direction);
		else
			color = SAMPLE_X_LINEAR_CLAMP(_Source, float3(i.localTexcoord.xy, 0), i.direction);

		if (_Radius == 0)
			return color;

		color *= gaussianWeights[0];

		for (int j = 1; j < SAMPLE_COUNT; j++)
		{
			float3 uvOffset = direction * j * (_Radius / _CustomRenderTextureWidth) / SAMPLE_COUNT;
			float cubemapDirectionOffset = j * (_Radius / (1.0 * _CustomRenderTextureWidth)) / SAMPLE_COUNT * 360; // humm ?
			float3 positiveDirectionOffset = Rotate(direction, i.direction, cubemapDirectionOffset);
			float3 negativeDirectionOffset = Rotate(direction, i.direction, -cubemapDirectionOffset);

			if (sampleSelf)
			{
				color += SAMPLE_SELF_LINEAR_CLAMP(i.localTexcoord.xyz + uvOffset, positiveDirectionOffset) * gaussianWeights[j];
				color += SAMPLE_SELF_LINEAR_CLAMP(i.localTexcoord.xyz - uvOffset, negativeDirectionOffset) * gaussianWeights[j];
			}
			else
			{
				color += SAMPLE_X_LINEAR_CLAMP(_Source, i.localTexcoord.xyz + uvOffset, positiveDirectionOffset) * gaussianWeights[j];
				color += SAMPLE_X_LINEAR_CLAMP(_Source, i.localTexcoord.xyz - uvOffset, negativeDirectionOffset) * gaussianWeights[j];
			}
		}

		return color;
	}

	ENDCG

	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			Name "Vertical Blur"

			CGPROGRAM
			float4 mixture(v2f_customrendertexture i) : SV_Target
			{
				return GaussianBlur(i, float3(1, 0, 0), false);
			}
			ENDCG
		}

		Pass
		{
			Name "Horizontal Blur"

			CGPROGRAM
			float4 mixture(v2f_customrendertexture i) : SV_Target
			{
				return GaussianBlur(i, float3(0, 1, 0), true);
			}
			ENDCG
		}

		Pass
		{
			Name "Depth Blur"

			CGPROGRAM
			float4 mixture(v2f_customrendertexture i) : SV_Target
			{
				return GaussianBlur(i, float3(0, 0, 1), true);
			}
			ENDCG
		}
	}
}
