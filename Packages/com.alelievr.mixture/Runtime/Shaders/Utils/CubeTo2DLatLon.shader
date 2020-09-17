Shader "Hidden/Mixture/CubeTo2DLatLon"
{	
	Properties
	{
			_Input("Input",Cube) = "white" {}
	}
		SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#include "Packages/com.alelievr.mixture/Runtime/Shaders/MixtureFixed.cginc"
			#pragma vertex CustomRenderTextureVertexShader
			#pragma fragment MixtureFragment
			#pragma target 3.0

			UNITY_DECLARE_TEXCUBE(_Input);

			float4 mixture(v2f_customrendertexture i) : SV_Target
			{
				return UNITY_SAMPLE_TEXCUBE(_Input, LatlongToDirectionCoordinate(i.localTexcoord.xy));
			}
			ENDCG
		}
	}
}
