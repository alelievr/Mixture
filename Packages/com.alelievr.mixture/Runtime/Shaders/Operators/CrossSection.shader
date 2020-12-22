Shader "Hidden/Mixture/CrossSection"
{	
	Properties
	{
		// By default a shader node is supposed to handle all the input texture dimension, we use a prefix to determine which one is used
		[InlineTexture]_Source_2D("Source", 2D) = "white" {}
		[InlineTexture]_Source_3D("Source", 3D) = "white" {}
		[InlineTexture]_Source_Cube("Source", Cube) = "white" {}

		// Other parameters
		[ToolTip(Slice of the inptu texture in the Y axis, between 0 and 1)]_Slice("Slice", Range(0, 1)) = 0
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
			float _Slice;

			float4 mixture (v2f_customrendertexture i) : SV_Target
			{
				// TODO: slice in 3D and cubemaps

				float3 uv = float3(i.localTexcoord.x, _Slice, i.localTexcoord.z);

				float4 value = SAMPLE_X(_Source, uv, i.direction);

				return smoothstep(value, 0, float4(i.localTexcoord.yyyy));
			}
			ENDHLSL
		}
	}
}
