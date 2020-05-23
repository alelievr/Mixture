Shader "Hidden/Mixture/HeightToNormal"
{	
	Properties
	{
		[InlineTexture]_Source_2D("Input", 2D) = "white" {}
		[MixtureChannel]_Channel("Height Channel", Float) = 3
		[Range]_Scale("Scale", Range(0.001,1.0)) = 1.0
		[Enum(UnsignedNormalized,0,Signed,1)]_OutputRange("Output Range", Float) = 1.0
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

            #pragma shader_feature CRT_2D

			TEXTURE_SAMPLER_X(_Source);
			float _Channel;
			float _Scale;
			float _OutputRange;

			float sampleHeight(float2 uv)
			{
				return SAMPLE_X(_Source, float3(uv, 0), float3(0, 0, 0))[(uint)_Channel];
			}

			float2 gradient(float2 uv, float2 offset)
			{
				float pu = sampleHeight(uv + offset * float2(0.5, 0.0));
				float pv = sampleHeight(uv + offset * float2(0.0, 0.5));
				float nu = sampleHeight(uv - offset * float2(0.5, 0.0));
				float nv = sampleHeight(uv - offset * float2(0.0, 0.5));

				return float2(pu - nu, pv - nv) / offset;
			}

			float4 mixture(v2f_customrendertexture i) : SV_Target
			{
				float2 dduv = 1.0 / float2(_CustomRenderTextureInfo.xy);
				float2 source = gradient(i.localTexcoord.xy, dduv); 
				_Scale = 1.0 / _Scale;
				float3 dx = float3(_Scale, 0.0, source.x);
				float3 dy = float3(0.0, _Scale, source.y);

				float3 output = normalize(cross(dx, dy));
				if (_OutputRange == 0.0)
					output = output * 0.5 + 0.5;

				return float4(output, 1);
			}
			ENDCG
		}
	}
}
