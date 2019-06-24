Shader "Hidden/Mixture/Blend"
{	
	Properties
	{
		[MixtureTexture2D]_Source("Source", 2D) = "white" {}
		[MixtureTexture2D]_Target("Target", 2D) = "white" {}
		[MixtureTexture2D]_Mask("Mask", 2D) = "white" {}
		[Enum(Blend,0,Additive,1,Multiplicative,2)]_BlendMode("Blend Mode", Float) = 0
		[Enum(Alpha,0,PerChannel,1)]_MaskMode("Mask Mode", Float) = 0
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment mixture

			#include "UnityCG.cginc"

			#define USE_UV
			#include "MixtureFixed.cginc"

			sampler2D _Source;
			sampler2D _Target;
			sampler2D _Mask;
			float _BlendMode;
			float _MaskMode;

			float4 mixture (MixtureInputs i) : SV_Target
			{
				float4 uv = float4(i.uv.x, i.uv.y, 0, 0);

				float4	source	= tex2Dlod(_Source, uv);
				float4	target	= tex2Dlod(_Target, uv);
				float4	mask;
				
				uint maskMode = (uint)_MaskMode;

				switch(maskMode)
				{
					case 0 : mask= tex2Dlod(_Mask, uv).aaaa; break;
					case 1 : mask= tex2Dlod(_Mask, uv).rgba; break;
				}

				uint mode = (uint)_BlendMode;
				switch (mode)
				{
					default:
					case 0: return lerp(source, target, mask);
					case 1: return lerp(source, source + target, mask);
					case 2: return lerp(source, source * target, mask);
				}
				// Should not happen but hey...
				return float4(1.0, 0.0, 1.0, 1.0);
			}
			ENDCG
		}
	}
}
