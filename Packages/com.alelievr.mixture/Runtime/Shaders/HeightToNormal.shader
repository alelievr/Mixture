Shader "Hidden/Mixture/HeightToNormal"
{	
	Properties
	{
		[InlineTexture]_Source_2D("Input", 2D) = "white" {}
		[Enum(Tangent, 0, Object, 1)] _OutputSpace("Output Space", Float) = 0.0
		[MixtureChannel]_Channel("Height Channel", Float) = 0
		[Range]_Strength("Scale", Range(0.001, 4.0)) = 1.0
		_MaxHeight("Max Height", Float) = 1.0
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
			#pragma enable_d3d11_debug_symbols

            #pragma shader_feature CRT_2D

			TEXTURE_X(_Source);
			float4 _Source_2D_TexelSize;
			float _Channel;
			float _Strength;
			float _OutputRange;
			float _OutputSpace;
			float _MaxHeight;

			float sampleHeight(float2 uv)
			{
				// Patch UVs to avoid sampling artifacts:
				uv = uv*_Source_2D_TexelSize.zw + 0.5;
				float2 iuv = floor( uv );
				float2 fuv = frac( uv );
				uv = iuv + fuv*fuv*(3.0-2.0*fuv); // fuv*fuv*fuv*(fuv*(fuv*6.0-15.0)+10.0);;
				uv = (uv - 0.5) / _Source_2D_TexelSize.zw;

				return SAMPLE_X_LINEAR_CLAMP(_Source, uv, 0)[(uint)_Channel] * 256 / _MaxHeight;
			}

			float3 normal(float2 step, float2 uv)
			{
				float tl = abs(sampleHeight(float2(uv.x - step.x, uv.y + step.y)).r);
				float l  = abs(sampleHeight(float2(uv.x - step.x, uv.y)).r);
				float bl = abs(sampleHeight(float2(uv.x - step.x, uv.y - step.y)).r);
				float t  = abs(sampleHeight(float2(uv.x, uv.y + step.y)).r);
				float b  = abs(sampleHeight(float2(uv.x, uv.y - step.y)).r);
				float tr = abs(sampleHeight(float2(uv.x + step.x, uv.y + step.y)).r);
				float r  = abs(sampleHeight(float2(uv.x + step.x, uv.y)).r);
				float br = abs(sampleHeight(float2(uv.x + step.x, uv.y - step.y)).r);

				float dx = tl + l * 2.0 + bl - tr - r * 2.0 - br;
				float dy = tl + t * 2.0 + tr - bl - b * 2.0 - br;

				return normalize(float3(dx, dy, 1 / _Strength));
			}

			float4 mixture(v2f_customrendertexture i) : SV_Target
			{
				float2 dduv = _Source_2D_TexelSize.xy;
				float3 output = normal(dduv * 1.5, i.localTexcoord.xy);

				if (_OutputSpace == 0.0)
					output.xyz = float3(output.x, -output.y, output.z);

				if (_OutputRange == 0.0)
					output = output * 0.5 + 0.5;

				return float4(output, 1);
			}
			ENDCG
		}
	}
}
