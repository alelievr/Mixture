Shader "Hidden/Mixture/Invert"
{	
	Properties
	{
		[InlineTexture]_Source_2D("Input", 2D) = "white" {}
		[InlineTexture]_Source_3D("Input", 3D) = "white" {}
		[InlineTexture]_Source_Cube("Input", Cube) = "white" {}

		[MaterialToggle] _Hue("Hue", Float) = 1.0
		[MaterialToggle] _Saturation("Saturation", Float) = 1.0
		[MaterialToggle] _Value("Value", Float) = 1.0
		[MaterialToggle] _Alpha("Alpha", Float) = 1.0
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

            #pragma shader_feature CRT_2D CRT_3D CRT_CUBE

			TEXTURE_SAMPLER_X(_Source);

			float _Hue;
			float _Saturation;
			float _Value;
			float _Alpha;

			float4 mixture(v2f_customrendertexture i) : SV_Target
			{
				float4 source = SAMPLE_X(_Source, i.localTexcoord.xyz, i.direction);
				bool ih = _Hue == 1.0;
				bool is = _Saturation == 1.0;
				bool iv = _Value == 1.0;

				if(ih && is && iv)
				{
					source.xyz = 1.0f - source.xyz;
				}
				else
				{
					float3 source_hsv = RGBtoHSV(source.rgb);
					if (ih) source_hsv.x = 1.0 - source_hsv.x;
					if (is) source_hsv.y = 1.0 - source_hsv.y;
					if (iv) source_hsv.z = 1.0 - source_hsv.z;

					source.xyz = HSVtoRGB(source_hsv);
				}

				if (_Alpha)
					source.a = 1.0f - source.a;
				
				return source;
			}
			ENDHLSL
		}
	}
}
