Shader "Hidden/Mixture/Mask"
{	
	Properties
	{
		[InlineTexture]_Target_2D("Target", 2D) = "white" {}
		[InlineTexture]_Target_3D("Target", 3D) = "white" {}
		[InlineTexture]_Target_Cube("Target", Cube) = "white" {}

		[InlineTexture]_Source_2D("Input", 2D) = "white" {}
		[InlineTexture]_Source_3D("Input", 3D) = "white" {}
		[InlineTexture]_Source_Cube("Input", Cube) = "white" {}

		[MixtureSwizzle]_Mask("Alpha", Float) = 3

		_Custom("Custom", Range(0.0,1.0)) = 0.3333
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#include "MixtureFixed.cginc"
			#include "UnityCustomRenderTexture.cginc"
            #pragma vertex CustomRenderTextureVertexShader
			#pragma fragment mixture
			#pragma target 3.0

            #pragma multi_compile CRT_2D CRT_3D CRT_CUBE

			TEXTURE_X(_Target);
			TEXTURE_X(_Source);

			float _Mask;
			float _Custom;

			float Swizzle(float4 sourceValue, uint mode, float custom)
			{
				switch (mode)
				{
				case 0: return sourceValue.x;
				case 1: return sourceValue.y;
				case 2: return sourceValue.z;
				case 3: return sourceValue.w;
				default:
				case 4: return 0.0f;
				case 5: return 0.5f;
				case 6: return 1.0f;
				case 7: return custom;
				}
				return 0;
			}

			float4 mixture (v2f_customrendertexture i) : SV_Target
			{
				float4 source = SAMPLE_X(_Source, float3(i.localTexcoord.xy, 0), i.direction);
				float4 target = SAMPLE_X(_Target, float3(i.localTexcoord.xy, 0), i.direction);

				float a = Swizzle(source, _Mask, _Custom);
				return float4(target.r, target.g, target.b, a);
			}
			ENDCG
		}
	}
}
