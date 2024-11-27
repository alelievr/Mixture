Shader "Hidden/Mixture/SuperShape"
{
	Properties
	{
		[InlineTexture]_UV_2D("UV", 2D) = "uv" {}
		[InlineTexture]_UV_3D("UV", 3D) = "uv" {}
		[InlineTexture]_UV_Cube("UV", Cube) = "uv" {}

		[Tooltip(Controls the overall roundness and symmetry of the shape)]
		_N1("N1", Float) = 5.0
		[Tooltip(Affects the depth of curves between shape vertices)]
		_N2("N2", Float) = 1.0
		[Tooltip(Controls the sharpness of the shapes points)]
		_N3("N3", Float) = 10.0
		[Tooltip(Number of repetitions or sides in the shape)]
		_M("M", Float) = 8.0
		[Tooltip(Horizontal scale factor of the shape)]
		_A("A", Float) = 0.8
		[Tooltip(Vertical scale factor of the shape)]
		_B("B", Float) = 0.8
		[Tooltip(Overall size of the shape)]
		_Scale("Scale", Float) = 0.4
		[Tooltip(Rotation angle of the shape (in radians))]
		_Rotation("Rotation", Float) = 0.0
		
		[Tooltip(Color used for the interior of the shape)]
		_ColorInside("Inside Color", Color) = (0.65, 0.85, 1.0, 1.0)
		[Tooltip(Color used for the exterior of the shape)]
		_ColorOutside("Outside Color", Color) = (0.9, 0.6, 0.3, 1.0)
		[Tooltip(Controls how many lines appear in the pattern)]
		_LineFrequency("Line Frequency", Float) = 150.0
		[Tooltip(Controls how strong visible the line pattern is)]
		_LineDefinition("Line Definition", Float) = 1.0
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
			#pragma shader_feature _ USE_CUSTOM_UV

			TEXTURE_SAMPLER_X(_UV);
			
			float _N1, _N2, _N3, _M, _A, _B, _Scale, _Rotation;
			float4 _ColorInside, _ColorOutside;
			float _LineFrequency, _LineDefinition;

			#define TAU 6.2831855

			float2 rotate2D(float2 p, float theta) 
			{
				float c = cos(theta);
				float s = sin(theta);
				return float2(
					p.x * c - p.y * s,
					p.x * s + p.y * c
				);
			}

			float gielisFormula(float teta, float3 n, float3 abm)
			{
				float b1 = pow(abs(cos((abm.z*teta)/4.0) / abm.x), n.y);
				float b2 = pow(abs(sin((abm.z*teta)/4.0) / abm.y), n.z);
				return pow(b1+b2, -1.0/n.x);
			}

			float calc2DPointPolarAngle(float2 p)
			{
				return atan2(p.y, p.x);
			}

			float sdSuperShape2D(float2 samplePoint) 
			{
				samplePoint = rotate2D(samplePoint, _Rotation);
				float polarAngle = calc2DPointPolarAngle(samplePoint);
				
				float r = gielisFormula(polarAngle, float3(_N1, _N2, _N3), float3(_A, _B, _M));
				return length(samplePoint)-(r*_Scale);
			}

			float3 colorCorrection(float3 col)
			{
				// gain and lift
				float gain = 1.8;
				float lift = 0.25;
				col = (col * (gain - lift)) + lift;        
				
				// gamma
				float gammaInv = 1.0;
				col = pow(col, float3(gammaInv, gammaInv, gammaInv));
				
				// saturation
				float grayCol = col.x*0.299 + col.y*0.587 + col.z*0.114;
				float sat = 2.8;
				col = sat * col + (1.0 - sat) * grayCol;
				
				// expansion
				float high = 1.1;
				float low = 0.15;
				col = (col-low)/(high-low);
				
				return col;
			}

			float calc3DSphericalAngles(float3 p, out float theta, out float phi)
			{
				float r = length(p);
				theta = acos(p.z / r); // polar angle (theta) [0, pi]
				phi = atan2(p.y, p.x);  // azimuthal angle (phi) [-pi, pi]
				return r;
			}

			float3 rotate3D(float3 p, float3 rotation)
			{
				float3 sinR, cosR;
				sincos(rotation, sinR, cosR);
				
				// Rotate around X
				float3 p1 = p;
				p1.y = p.y * cosR.x - p.z * sinR.x;
				p1.z = p.y * sinR.x + p.z * cosR.x;
				
				// Rotate around Y
				float3 p2 = p1;
				p2.x = p1.x * cosR.y + p1.z * sinR.y;
				p2.z = -p1.x * sinR.y + p1.z * cosR.y;
				
				// Rotate around Z
				float3 p3 = p2;
				p3.x = p2.x * cosR.z - p2.y * sinR.z;
				p3.y = p2.x * sinR.z + p2.y * cosR.z;
				
				return p3;
			}

			float superShape3D(float theta, float phi, float3 n1, float3 n2, float3 n3, float3 m, float3 a, float3 b)
			{
				float r1 = gielisFormula(phi, n1, float3(a.x, b.x, m.x));
				float r2 = gielisFormula(theta, n2, float3(a.y, b.y, m.y));
				
				float x = r1 * sin(theta) * cos(phi);
				float y = r1 * sin(theta) * sin(phi);
				float z = r2 * cos(theta);
				
				return length(float3(x, y, z));
			}

			float sdSuperShape(float3 p)
			{
				p = rotate3D(p, float3(_Rotation, _Rotation * 0.7, _Rotation * 1.3));
				
				float theta, phi;
				float r = calc3DSphericalAngles(p, theta, phi);
				
				float3 n1 = float3(_N1, _N2, _N3);
				float3 n2 = float3(_N1 * 0.8, _N2 * 1.2, _N3 * 0.9);
				float3 n3 = float3(_N1 * 1.2, _N2 * 0.8, _N3 * 1.1);
				float3 m = float3(_M, _M * 0.8, _M * 1.2);
				float3 a = float3(_A, _A * 1.1, _A * 0.9);
				float3 b = float3(_B, _B * 0.9, _B * 1.1);
				
				float shape = superShape3D(theta, phi, n1, n2, n3, m, a, b);
				return r - shape * _Scale;
			}

			float4 mixture(v2f_customrendertexture i) : SV_Target
			{
				#ifdef USE_CUSTOM_UV
					float4 uv = SAMPLE_X_NEAREST_CLAMP(_UV, i.localTexcoord.xyz, i.direction);
				#else
					float4 uv = float4(GetDefaultUVs(i), 1);
				#endif

				float ss;
				#if defined(CRT_3D) || defined(CRT_CUBE)
					// Transform UV to centered 3D coordinates
					float3 p = (uv.xyz * 2.0 - 1.0);
					ss = sdSuperShape(p);
				#else
					// Transform UV to centered 2D coordinates
					float2 p = (uv.xy * 2.0 - 1.0);
					ss = sdSuperShape2D(p);
				#endif
				
				float3 col = (ss > 0.0) ? _ColorOutside.rgb : _ColorInside.rgb;
				col *= 1.0 - exp(-0.8*abs(ss));
				col *= 0.8 + _LineDefinition*cos(_LineFrequency*ss);
				
				//col = colorCorrection(col);

				return float4(col, 1.0);
			}
			ENDHLSL
		}
	}
}
