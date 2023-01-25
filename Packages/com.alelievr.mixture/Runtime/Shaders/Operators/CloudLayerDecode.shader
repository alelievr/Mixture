Shader "Hidden/Mixture/CloudLayerDecode"
{	
	Properties
	{
		_Source("Source", 2D) = "white" {}
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
            #pragma shader_feature CRT_2D

			// This macro will declare a version for each dimention (2D, 3D and Cube)
			Texture2D _Source;
			sampler sampler_Source;

			float _UpperHemisphereOnly;

			float4 mixture (v2f_customrendertexture i) : SV_Target
			{
				float2 uv = DirectionToLatLongCoordinate(i.direction);
				if (_UpperHemisphereOnly)
					uv.y = MixtureRemap(uv.y, 0, 1, -1, 1);

				if (uv.y < 0)
					return 0;

				return _Source.SampleLevel(sampler_Source, uv, 0);
			}
			ENDHLSL
		}
	}
}
