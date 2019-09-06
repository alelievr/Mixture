Shader "Hidden/Mixture/Distance"
{	
	Properties
	{
		// By default a shader node is supposed to handle all the input texture dimension, we use a prefix to determine which one is used
		[InlineTexture]_Source_2D("Source", 2D) = "white" {}
		[InlineTexture]_Source_3D("Source", 3D) = "white" {}
		[InlineTexture]_Source_Cube("Source", Cube) = "white" {}

		// Other parameters
		[Range]_Threshold("Threshold", Range(0, 1)) = 0.5
		[Range]_Radius("Radius", Range(0, 32)) = 4
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#include "Packages/com.alelievr.mixture/Runtime/Shaders/MixtureFixed.cginc"
			#include "UnityCustomRenderTexture.cginc"
            #pragma vertex CustomRenderTextureVertexShader
			#pragma fragment mixture
			#pragma target 3.0

			// The list of defines that will be active when processing the node with a certain dimension
            #pragma multi_compile CRT_2D CRT_3D CRT_CUBE

			// This macro will declare a version for each dimention (2D, 3D and Cube)
			TEXTURE_X(_Source);
			float _Threshold;
			float _Radius;

			float4 mixture (v2f_customrendertexture crt) : SV_Target
			{
				// The SAMPLE_X macro handles sampling for 2D, 3D and cube textures
				float4 input = LOAD_X(_Source, crt.localTexcoord.xyz, crt.direction);
				float4 color = input;

				int i = -_Radius, j = -_Radius, k = -_Radius;
				for (; i <= _Radius; i++)
				{
					for (; j <= _Radius; j++)
#if defined(CRT_3D)
						for (; k <= _Radius; k++)
#endif
						{
							if (i == 0 && j == 0 && k == 0)
								continue;

							float uvOffset = float3(i, j, k) / float3(_CustomRenderTextureWidth, _CustomRenderTextureHeight, _CustomRenderTextureDepth);
							float4 neighbour = LOAD_X(_Source, crt.localTexcoord.xyz + uvOffset, crt.direction);

							if (any(neighbour.r < _Threshold))
							{
								color = neighbour;
								// break;
							}
						}
				}

				return color;
			}
			ENDCG
		}
	}
}
