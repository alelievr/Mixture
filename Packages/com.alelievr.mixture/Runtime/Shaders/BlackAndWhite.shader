Shader "Hidden/Mixture/BlackAndWhite"
{	
	Properties
	{
		[InlineTexture]_Source_2D("Input", 2D) = "white" {}
		[InlineTexture]_Source_3D("Input", 3D) = "white" {}
		[InlineTexture]_Source_Cube("Input", Cube) = "white" {}

		[MixtureVector3]_ColorNorm("Lum Factors",Vector) = (0.299, 0.587, 0.114)
		_ColorFilter("Color Filter", Color) = (1.0,1.0,1.0,1.0)
		[Range]_KeepLuminance("Keep Lum", Range(0.0,1.0)) = 1.0
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#include "Packages/com.alelievr.mixture/Runtime/Shaders/MixtureFixed.cginc"
            #pragma vertex CustomRenderTextureVertexShader
			#pragma fragment MixtureFragment
			#pragma target 3.0

            #pragma shader_feature CRT_2D CRT_3D CRT_CUBE

			TEXTURE_SAMPLER_X(_Source);

			float4 _ColorNorm;
			float4 _ColorFilter;
			float  _KeepLuminance;

			float Luminance(float3 source)
			{
				return source.r * _ColorNorm.x + source.g * _ColorNorm.g + source.b * _ColorNorm.b;
			}

			float4 mixture(v2f_customrendertexture i) : SV_Target
			{
				_ColorNorm = normalize(_ColorNorm);
				float4 source = SAMPLE_X(_Source, i.localTexcoord.xyz, i.direction);

				_ColorFilter /= lerp(1.0, Luminance(_ColorFilter.rgb), _KeepLuminance);

				source.rgb *= _ColorFilter;

				float n = Luminance(source);
				return float4(n,n,n,source.a);
			}
			ENDCG
		}
	}
}
