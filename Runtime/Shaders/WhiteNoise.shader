Shader "Hidden/Mixture/WhiteNoise"
{	
	Properties
	{
		[Enum(Single Channel, 0, RGB, 1, RGBA, 2)] _Mode("Mode", Float) = 0
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

			float _Mode;

			float WhiteNoise(float3 uvs)
			{
				float3 smallValue = sin(uvs);
				float random = dot(smallValue, float3(12.9898, 78.233, 37.719));
				random = frac(sin(random) * 143758.5453);
				return random;
			}

			float4 mixture (v2f_customrendertexture i) : SV_Target
			{
				switch (_Mode)
				{
					default:
					case 0:
						return float4(WhiteNoise(i.localTexcoord.xyz + i.direction).xxx, 1);
					case 1:
						return float4(
							WhiteNoise(i.localTexcoord.xyz + i.direction),
							WhiteNoise(i.localTexcoord.xyz + i.direction - float3(-10.5645, 1.548, 6.484)),
							WhiteNoise(i.localTexcoord.xyz + i.direction + float3(6.4548, -6.5854, 1.564)),
							1
						);
					case 2:
						return float4(
							WhiteNoise(i.localTexcoord.xyz + i.direction),
							WhiteNoise(i.localTexcoord.xyz + i.direction - float3(-10.5645, 1.548, 6.484)),
							WhiteNoise(i.localTexcoord.xyz + i.direction + float3(6.4548, -6.5854, 1.564)),
							WhiteNoise(i.localTexcoord.xyz + i.direction - float3(3.6844, 4.54589, -4.456))
						);
				}
			}
			ENDCG
		}
	}
}
