Shader "Hidden/Mixture/CloudLayerEncode"
{	
	Properties
	{
		_Source("Source", Cube) = "white" {}
		[Tooltip(Is the map encoded for Upper Hemisphere cloud layer setting)][Toggle]_UpperHemisphereOnly("Upper Hemisphere Only", Float) = 1
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
			TextureCube _Source;
			sampler sampler_Source;
			float _UpperHemisphereOnly;

			float4 mixture (v2f_customrendertexture i) : SV_Target
			{
				float2 uv = i.localTexcoord.xy;

				if (_UpperHemisphereOnly)
					uv.y = MixtureRemap(uv.y, 0, 1, 0.5, 1);

				float3 dir = LatlongToDirectionCoordinate(uv);
				return _Source.SampleLevel(sampler_Source, dir, 0);
			}
			ENDHLSL
		}
	}
}
