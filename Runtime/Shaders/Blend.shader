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
			#include "MixtureFixed.cginc"
			#include "UnityCustomRenderTexture.cginc"
            #pragma vertex CustomRenderTextureVertexShader
			#pragma fragment mixture
			#pragma target 3.0

			TEXTURE2D(_Source);
			TEXTURE2D(_Target);
			TEXTURE2D(_Mask);
			float _BlendMode;
			float _MaskMode;

			float4 mixture (v2f_customrendertexture i) : SV_Target
			{
				float4	source	= tex2Dlod(_Source, float4(i.localTexcoord.xy, 0, 0));
				float4	target	= tex2Dlod(_Target, float4(i.localTexcoord.xy, 0, 0));
				float4	mask = 0;
				
				uint maskMode = (uint)_MaskMode;

				switch(maskMode)
				{
					case 0 : mask= tex2Dlod(_Mask, float4(i.localTexcoord.xy, 0, 0)).aaaa; break;
					case 1 : mask= tex2Dlod(_Mask, float4(i.localTexcoord.xy, 0, 0)).rgba; break;
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
