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
			#include "Packages/com.alelievr.mixture/Runtime/Shaders/MixtureFixed.cginc"
			#pragma vertex CustomRenderTextureVertexShader
			#pragma fragment MixtureFragment
			#pragma target 3.0

			#pragma shader_feature CRT_2D CRT_3D CRT_CUBE
	
			float _Mode;
			float4 _Color1;
			float4 _Color2;

			float4 mixture (v2f_customrendertexture IN) : SV_Target
			{
				float3 uv = IN.localTexcoord.xyz;
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
