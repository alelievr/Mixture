Shader "Hidden/Mixture/CubeTo2DLatLon"
{	
	Properties
	{
		_Input("Input", Cube) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 100

		Pass
		{
			HLSLPROGRAM
			#include "Packages/com.alelievr.mixture/Runtime/Shaders/MixtureFixed.hlsl"
			#pragma vertex CustomRenderTextureVertexShader
			#pragma fragment MixtureFragment
			#pragma target 3.0

			TextureCube _Input;
			sampler sampler_Input;

			float4 mixture(v2f_customrendertexture i) : SV_Target
			{
				return _Input.SampleLevel(sampler_Input, LatlongToDirectionCoordinate(i.localTexcoord.xy), 0);
			}
			ENDHLSL
		}
	}
}
