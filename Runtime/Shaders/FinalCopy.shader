Shader "Hidden/Mixture/FinalCopy"
{	
	Properties
	{
		_Source2D("Source", 2D) = "white" {}
		_Source3D("Source", 3D) = "white" {}
		_SourceCube("Source", Cube) = "white" {}
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
			
            #pragma multi_compile CRT_2D CRT_3D CRT_CUBE

			#pragma fragment mixture
            #pragma vertex CustomRenderTextureVertexShader
			#pragma target 3.0

			TEXTURE2D(_Source2D);
			TEXTURE3D(_Source3D);
			TEXTURECUBE(_SourceCube);

			static const float3 faceVectors[6] = {
				float3(1, 0, 0),
				float3(-1, 0, 0),
				float3(0, 1, 0),
				float3(0, -1, 0),
				float3(0, 0, 1),
				float3(0, 0, -1),
			};

			float4 mixture (v2f_customrendertexture IN) : SV_Target
			{
#if CRT_2D
				return tex2Dlod(_Source2D, float4(IN.localTexcoord.xy, 0, 0));
#elif CRT_3D
				return tex3Dlod(_Source3D, float4(IN.localTexcoord.xy, _CustomRenderTexture3DSlice, 0));
#else // CUBEMAP
				return texCUBElod(_SourceCube, float4(IN.direction, 0));
#endif
			}
			ENDCG
		}
	}
}
