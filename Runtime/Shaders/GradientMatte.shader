Shader "Hidden/Mixture/GradientMatte"
{	
	Properties
	{
		[Enum(Horizontal,0,Vertical,1,Radial,2,Circular,3)]_Mode("Gradient Type", Float) = 0
		[HDR]_Color1("Color 1", Color) = (0.0,0.0,0.0,0.0)
		[HDR]_Color2("Color 2", Color) = (1.0,1.0,1.0,1.0)
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
			
			float _Mode;
			float4 _Color1;
			float4 _Color2;

			float4 mixture (MixtureInputs i) : SV_Target
			{
				float2 uv = float2(i.uv.x, i.uv.y);
				float gradient = 0.0f;

				uint mode = (uint)_Mode;
				switch (mode)
				{
					case 0: gradient = uv.x; break;
					case 1: gradient = uv.y; break;
					case 2: uv -= 0.5; gradient = pow(saturate(1.0 - (dot(uv, uv) * 4.0)), 2.0); break;
					case 3: uv -= 0.5; gradient = saturate((atan2(uv.y, uv.x) / 6.283185307179586476924) + 0.5); break;
				}
				return lerp(_Color1,_Color2,gradient);
			}
			ENDCG
		}
	}
}
