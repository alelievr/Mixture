Shader "Hidden/Mixture/Polygon2D"
{	
	Properties
	{
		// By default a shader node is supposed to handle all the input texture dimension, we use a prefix to determine which one is used
		[InlineTexture]_UV_2D("UV", 2D) = "uv" {}
		[InlineTexture]_UV_3D("UV", 3D) = "uv" {}
		[InlineTexture]_UV_Cube("UV", Cube) = "uv" {}

		[Tooltip(Color inside the polygon)]
		_InnerColor("Inner Color", Color) = (1, 1, 1, 1)
		[Tooltip(Color outside of the polygon)]
		_OuterColor("Outer Color", Color) = (0, 0, 0, 0)

		[Tooltip(Number of sides of the polygon, can be a non integer value)]
		_SideCount("Side Count", Float) = 3

		[Tooltip(Size of the polygon)]
		_Size("Size", Range(0, 2)) = 0.7
		[Tooltip(Smooth the polygon edges and creates a gradient between the color inside and outside of the polygon)]
		_Smooth("Smooth", Range(0, 1)) = 0

		[Tooltip(Make a star shape out of the current polygon)]
		[ShowInInspector]_Starryness("Starryness", Range(0, 1)) = 0
		[Tooltip(Select the output mode. Can be either Color to output the color of the polygon or DistanceField to output the signed distance field of the polygon)]
		[ShowInInspector][Enum(Color, 0, DistanceField, 1)]_Mode("Mode", Float) = 0
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
            #pragma shader_feature CRT_2D CRT_3D CRT_CUBE
			#pragma shader_feature _ USE_CUSTOM_UV

			// This macro will declare a version for each dimention (2D, 3D and Cube)
			TEXTURE_SAMPLER_X(_UV);
			float _SideCount;
			float4 _InnerColor;
			float4 _OuterColor;
			float _Size;
			float _Starryness;
			float _Smooth;
			float _Mode;

			// Source: http://www.iquilezles.org/www/articles/distfunctions2d/distfunctions2d.htm

			float mod(float x, float y)
			{
				return x - y * floor(x / y);
			}

			// signed distance to a n-star polygon with external angle en
			float sdStar(float2 p, float r, float n, float m) // m=[2,n]
			{
				// these 4 lines can be precomputed for a given shape
				float an = PI / float(n);
				float en = PI / m;
				float2  acs = float2(cos(an),sin(an));
				float2  ecs = float2(cos(en),sin(en)); // ecs=float2(0,1) and simplify, for regular polygon,

				// reduce to first sector
				float bn = mod(atan2(p.x , p.y),2.0 * an) - an;
				p = length(p) * float2(cos(bn), abs(sin(bn)));

				// line sdf
				p -= r*acs;
				p += ecs*clamp( -dot(p,ecs), 0.0, r*acs.y/ecs.y);
				return length(p)*sign(p.x);
			}

			float4 mixture (v2f_customrendertexture i) : SV_Target
			{
#ifdef USE_CUSTOM_UV
				float4 uv = SAMPLE_X_NEAREST_CLAMP(_UV, i.localTexcoord.xyz, i.direction);
#else
				float4 uv = float4(GetDefaultUVs(i), 1);
#endif

				float faceCount = max(2, _SideCount);
				float distance = sdStar((uv * 2 - 1).xy, _Size, faceCount, lerp(2, faceCount, _Starryness));
				float smoothDistance = smoothstep(distance - _Smooth, distance, 0);

				switch (_Mode)
				{
					default:
					case 0: // Color
						return lerp(_OuterColor, _InnerColor, smoothDistance);
					case 1: // SDF
						return distance;
				}
			}
			ENDHLSL
		}
	}
}
