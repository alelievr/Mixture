Shader "Hidden/Mixture/EdgeDetect"
{	
	Properties
	{
		// By default a shader node is supposed to handle all the input texture dimension, we use a prefix to determine which one is used
		[InlineTexture]_Source_2D("Source", 2D) = "white" {}
		[InlineTexture]_Source_3D("Source", 3D) = "white" {}
		[InlineTexture]_Source_Cube("Source", Cube) = "white" {}

		_Step("Step", Range(0.01, 2)) = 1
		[Tooltip(Output color mode, it can either be white and black or input texture coor)][Enum(Edge, 0, ColorEdge, 1)] _Mode("Mode", Float) = 0
	}

	HLSLINCLUDE
	
	#include "Packages/com.alelievr.mixture/Runtime/Shaders/MixtureFixed.hlsl"
	#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

	#pragma target 3.0
	// The list of defines that will be active when processing the node with a certain dimension
	#pragma shader_feature CRT_2D CRT_3D CRT_CUBE
	#pragma vertex CustomRenderTextureVertexShader
	#pragma fragment MixtureFragment

	static float3x3 horizontalSobelMatrix = float3x3(
		 3,  10,  3,
		 0,   0,  0,
		-3, -10, -3
	);

	static float3x3 verticalSobelMatrix = float3x3(
		 3,  0,  -3,
		10,  0, -10,
		 3,  0,  -3
	);

	TEXTURE_SAMPLER_X(_Source);
	float _Step;
	float _Mode;

	float EdgeDetect(float3x3 pixels, bool h)
	{
		float3x3 m;
		
		if (h)
			m = horizontalSobelMatrix * pixels;
		else
			m = verticalSobelMatrix * pixels;

		float result = 0.0;

		for (int i = 0; i < 3; i++)
			for (int j = 0; j < 3; j++)
				result += m[i][j] * pixels[i][j];
			
		return result;
	}

	float SamplePixelLuminance(float3 direction, float3 uvs)
	{
		float3 rcpSize = rcp(float3(_CustomRenderTextureWidth, _CustomRenderTextureHeight, _CustomRenderTextureDepth));

#ifdef CRT_CUBE
		return 0;
		// TODO
#else
		return Luminance(SAMPLE_X(_Source, uvs + direction * rcpSize * _Step, float3(0, 0, 0)).rgb);
#endif
	}

	ENDHLSL

	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			Name "EdgeDetect"

			HLSLPROGRAM
			float4 mixture(v2f_customrendertexture i) : SV_Target
			{
				float3x3 horizontalPixels = float3x3(
					SamplePixelLuminance(float3(-1, -1, 0), i.localTexcoord.xyz), SamplePixelLuminance(float3( 0, -1, 0), i.localTexcoord.xyz), SamplePixelLuminance(float3( 1, -1, 0), i.localTexcoord.xyz),
					SamplePixelLuminance(float3(-1,  0, 0), i.localTexcoord.xyz), SamplePixelLuminance(float3( 0,  0, 0), i.localTexcoord.xyz), SamplePixelLuminance(float3( 1,  0, 0), i.localTexcoord.xyz),
					SamplePixelLuminance(float3(-1,  1, 0), i.localTexcoord.xyz), SamplePixelLuminance(float3( 0,  1, 0), i.localTexcoord.xyz), SamplePixelLuminance(float3( 1,  1, 0), i.localTexcoord.xyz)
				);

				float2 edgeValue = float2(EdgeDetect(horizontalPixels, true), EdgeDetect(horizontalPixels, false));
				float edge = length(edgeValue);

				switch (_Mode)
				{
					default:
					case 0:
						return float4(edge.xxx, 1);
					case 1:
						return SAMPLE_X(_Source, i.localTexcoord.xyz, i.direction) * edge;
				}
			}
			ENDHLSL
		}
	}
}
