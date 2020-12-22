Shader "Hidden/Mixture/BlackAndWhite"
{	
	Properties
	{
		[Tooltip(Source Texture)][InlineTexture]_Source_2D("Input", 2D) = "white" {}
		[Tooltip(Source Texture)][InlineTexture]_Source_3D("Input", 3D) = "white" {}
		[Tooltip(Source Texture)][InlineTexture]_Source_Cube("Input", Cube) = "white" {}

		[Enum(Perceptual Luminance, 0, D65 Luminance, 1, Custom Luminance, 2, Lightness, 3, Average, 4)]_LuminanceMode("Mode", Float) = 0
		[ShowInInspector][MixtureVector3]_ColorNorm("Lum Factors",Vector) = (0.299, 0.587, 0.114)
		_ColorFilter("Color Filter", Color) = (1.0,1.0,1.0,1.0)
		[Range]_KeepLuminance("Keep Lum", Range(0.0,1.0)) = 1.0
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

			float4 _ColorNorm;
			float4 _ColorFilter;
			float  _KeepLuminance;
			float  _LuminanceMode;

			float Luminance(float3 source)
			{
				if (_LuminanceMode == 3) // Lightness
					return (Max3(source.r, source.g, source.b) + Min3(source.r, source.g, source.b)) / 2;
				else if (_LuminanceMode == 4) // Average
					return (source.r + source.g + source.b) / 3;
				else // Luminance
					return dot(source, _ColorNorm.rgb);
			}

			float4 mixture(v2f_customrendertexture i) : SV_Target
			{
				_ColorNorm = normalize(_ColorNorm);
				float4 source = SAMPLE_X(_Source, i.localTexcoord.xyz, i.direction);

				_ColorFilter /= lerp(1.0, Luminance(_ColorFilter.rgb), _KeepLuminance);

				source.rgb *= _ColorFilter.rgb;

				float n = Luminance(source.rgb);
				return float4(n,n,n,source.a);
			}
			ENDHLSL
		}
	}
}
