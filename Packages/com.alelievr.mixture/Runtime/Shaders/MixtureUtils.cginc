#ifndef MIXTURE_UTILS
#define MIXTURE_UTILS

#ifndef UNITY_PI
#define UNITY_PI 3.14159265358979323846
#endif

#undef SAMPLE_DEPTH_TEXTURE
#undef SAMPLE_DEPTH_TEXTURE_LOD
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"

// Utility samplers:
sampler s_linear_clamp_sampler;
sampler s_linear_repeat_sampler;
sampler s_point_clamp_sampler;
sampler s_point_repeat_sampler;

// Macros
#define MERGE_NAME(p1,p2) p1##p2

#define TEXTURE_SAMPLER2D(name) sampler2D name
#define TEXTURE_SAMPLER3D(name) sampler3D name
#define TEXTURE_SAMPLERCUBE(name) samplerCUBE name

#define SAMPLE_2D(tex, uv) tex2Dlod(tex, float4(uv, 0))

#ifdef CRT_2D
	#define SAMPLE_X(tex, uv, dir) tex2Dlod(MERGE_NAME(tex,_2D), float4(uv, 0))
	#define SAMPLE_LOD_X(tex, uv, dir, lod) tex2Dlod(MERGE_NAME(tex,_2D), float4(uv, lod))
	#define SAMPLE_X_SAMPLER(tex, samp, uv, dir) SAMPLE_TEXTURE2D_LOD(MERGE_NAME(tex,_2D), samp, (uv).xy, 0)
	#define SAMPLE_X_SAMPLER_LOD(tex, samp, uv, dir, lod) SAMPLE_TEXTURE2D_LOD(MERGE_NAME(tex,_2D), samp, (uv).xy, lod)
	#define SAMPLE_X_LINEAR_CLAMP(tex, uv, dir)	SAMPLE_TEXTURE2D_LOD(MERGE_NAME(tex,_2D), s_linear_clamp_sampler, (uv).xy, 0)
	#define SAMPLE_LOD_X_LINEAR_CLAMP(tex, uv, dir, lod) SAMPLE_TEXTURE2D_LOD(MERGE_NAME(tex,_2D), s_linear_clamp_sampler, (uv).xy, lod)
	#define SAMPLE_X_NEAREST_CLAMP(tex, uv, dir) SAMPLE_TEXTURE2D_LOD(MERGE_NAME(tex,_2D), s_point_clamp_sampler, (uv).xy, 0)
	#define LOAD_X(tex, uv, dir) LOAD_TEXTURE2D_LOD(MERGE_NAME(tex,_2D), (uv).xy * float2(_CustomRenderTextureWidth, _CustomRenderTextureHeight), 0)
	#define SET_X(tex, uv, value) MERGE_NAME(tex,_2D)[uint2((uv).xy * float2(_CustomRenderTextureWidth, _CustomRenderTextureHeight))] = value
	#define FLOAT_X float2
	#define INT_X int2
	#define TEXTURE_TYPE Texture2D

	#define TEXTURE_SAMPLER_X(tex)	TEXTURE_SAMPLER2D(MERGE_NAME(tex,_2D))
	#define TEXTURE_X(name) TEXTURE2D(MERGE_NAME(name,_2D))
	#define RW_TEXTURE_X(type, name) RW_TEXTURE2D(type, MERGE_NAME(name,_2D))

	#define LOAD_SELF(uv, dir) LOAD_TEXTURE2D_LOD(_SelfTexture2D, (uv) * float2(_CustomRenderTextureWidth, _CustomRenderTextureHeight), 0);
	#define SAMPLE_SELF(uv, dir) SAMPLE_TEXTURE2D_LOD(_SelfTexture2D, sampler_SelfTexture2D, uv, 0)
	#define SAMPLE_SELF_LINEAR_CLAMP(uv, dir) SAMPLE_TEXTURE2D_LOD(_SelfTexture2D, s_linear_clamp_sampler, uv, 0)
	#define SAMPLE_SELF_SAMPLER(s, uv, dir) SAMPLE_TEXTURE2D_LOD(_SelfTexture2D, s, uv, 0)
#elif CRT_3D
	#define SAMPLE_X(tex, uv, dir)	tex3Dlod(MERGE_NAME(tex,_3D), float4(uv, 0))
	#define SAMPLE_LOD_X(tex, uv, dir, lod)	tex3Dlod(MERGE_NAME(tex,_3D), float4(uv, lod))
	#define SAMPLE_X_SAMPLER(tex, samp, uv, dir) SAMPLE_TEXTURE3D_LOD(MERGE_NAME(tex,_3D), samp, uv.xyz, 0)
	#define SAMPLE_X_SAMPLER_LOD(tex, samp, uv, dir, lod) SAMPLE_TEXTURE3D_LOD(MERGE_NAME(tex,_3D), samp, uv.xyz, lod)
	#define SAMPLE_X_LINEAR_CLAMP(tex, uv, dir)	SAMPLE_TEXTURE3D_LOD(MERGE_NAME(tex,_3D), s_linear_clamp_sampler, (uv).xyz, 0)
	#define SAMPLE_LOD_X_LINEAR_CLAMP(tex, uv, dir, lod) SAMPLE_TEXTURE3D_LOD(MERGE_NAME(tex,_3D), s_linear_clamp_sampler, uv.xyz, lod)
	#define SAMPLE_X_NEAREST_CLAMP(tex, uv, dir) SAMPLE_TEXTURE3D_LOD(MERGE_NAME(tex,_3D), s_point_clamp_sampler, (uv).xyz, 0)
	#define LOAD_X(tex, uv, dir) LOAD_TEXTURE3D_LOD(MERGE_NAME(tex,_3D), (uv) * float3(_CustomRenderTextureWidth, _CustomRenderTextureHeight, _CustomRenderTextureDepth), 0)
	#define SET_X(tex, uv, value) MERGE_NAME(tex,_3D)[uint3((uv).xyz * float3(_CustomRenderTextureWidth, _CustomRenderTextureHeight, _CustomRenderTextureDepth))] = value
	#define FLOAT_X float3
	#define INT_X int3
	#define TEXTURE_TYPE Texture3D

	#define TEXTURE_SAMPLER_X(tex)	TEXTURE_SAMPLER3D(MERGE_NAME(tex,_3D))
	#define TEXTURE_X(name) TEXTURE3D(MERGE_NAME(name,_3D))
	#define RW_TEXTURE_X(type, name) RW_TEXTURE3D(type, MERGE_NAME(name,_3D))

	#define LOAD_SELF(uv, dir) LOAD_TEXTURE3D_LOD(_SelfTexture3D, (uv) * float3(_CustomRenderTextureWidth, _CustomRenderTextureHeight, _CustomRenderTextureDepth), 0);
	#define SAMPLE_SELF(uv, dir) SAMPLE_TEXTURE3D_LOD(_SelfTexture3D, sampler_SelfTexture3D, uv, 0)
	#define SAMPLE_SELF_LINEAR_CLAMP(uv, dir) SAMPLE_TEXTURE3D_LOD(_SelfTexture3D, s_linear_clamp_sampler, uv, 0)
	#define SAMPLE_SELF_SAMPLER(s, uv, dir) SAMPLE_TEXTURE3D_LOD(_SelfTexture3D, s, uv, 0)
#elif CRT_CUBE
	#define SAMPLE_X(tex, uv, dir) texCUBElod(MERGE_NAME(tex,_Cube), float4(dir, 0))
	#define SAMPLE_LOD_X(tex, uv, dir, lod)	texCUBElod(MERGE_NAME(tex,_Cube), float4(dir, lod))
	#define SAMPLE_X_SAMPLER(tex, samp, uv, dir) SAMPLE_TEXTURECUBE_LOD(MERGE_NAME(tex,_Cube), samp, dir, 0)
	#define SAMPLE_X_SAMPLER_LOD(tex, samp, uv, dir, lod) SAMPLE_TEXTURECUBE_LOD(MERGE_NAME(tex,_Cube), samp, dir, lod)
	#define SAMPLE_X_LINEAR_CLAMP(tex, uv, dir)	SAMPLE_TEXTURECUBE_LOD(MERGE_NAME(tex,_Cube), s_linear_clamp_sampler, dir, 0)
	#define SAMPLE_LOD_X_LINEAR_CLAMP(tex, uv, dir, lod) SAMPLE_TEXTURECUBE_LOD(MERGE_NAME(tex,_Cube), s_linear_clamp_sampler, dir, lod)
	#define SAMPLE_X_NEAREST_CLAMP(tex, uv, dir) SAMPLE_TEXTURECUBE_LOD(MERGE_NAME(tex,_Cube), s_point_clamp_sampler, dir, 0)
	#define LOAD_X(tex, uv, dir) SAMPLE_X_NEAREST_CLAMP(tex, uv, dir) // there is no load on cubemaps
	#define FLOAT_X float3
	#define INT_X int3
	#define TEXTURE_TYPE TextureCube

	#define TEXTURE_SAMPLER_X(tex)	TEXTURE_SAMPLERCUBE(MERGE_NAME(tex,_Cube))
	#define TEXTURE_X(name) TEXTURECUBE(MERGE_NAME(name,_Cube))

	#define SAMPLE_SELF(uv, dir) SAMPLE_TEXTURECUBE_LOD(_SelfTextureCube, sampler_SelfTextureCube, dir, 0)
	#define SAMPLE_SELF_LINEAR_CLAMP(uv, dir) SAMPLE_TEXTURECUBE_LOD(_SelfTextureCube, s_linear_clamp_sampler, dir, 0)
	#define SAMPLE_SELF_SAMPLER(s, uv, dir) SAMPLE_TEXTURECUBE_LOD(_SelfTextureCube, s, dir, 0)
#endif

/////////////////////////////////////////////////////////////////////////
//																	   //
// HSV <-> RGB Conversion                                              //
// From https://chilliant.blogspot.com/2010/11/rgbhsv-in-hlsl.html     //
//																	   //
/////////////////////////////////////////////////////////////////////////

float3 HSVtoRGB(float3 HSV)
{
	float3 RGB = 0;
	float C = HSV.z * HSV.y;
	float H = HSV.x * 6;
	float X = C * (1 - abs(fmod(H, 2) - 1));
	if (HSV.y != 0)
	{
		float I = floor(H);
		if (I == 0) { RGB = float3(C, X, 0); }
		else if (I == 1) { RGB = float3(X, C, 0); }
		else if (I == 2) { RGB = float3(0, C, X); }
		else if (I == 3) { RGB = float3(0, X, C); }
		else if (I == 4) { RGB = float3(X, 0, C); }
		else { RGB = float3(C, 0, X); }
	}
	float M = HSV.z - C;
	return RGB + M;
}

float3 RGBtoHSV(float3 RGB)
{
	float3 HSV = 0;
	float M = min(RGB.r, min(RGB.g, RGB.b));
	HSV.z = max(RGB.r, max(RGB.g, RGB.b));
	float C = HSV.z - M;
	if (C != 0)
	{
		HSV.y = C / HSV.z;
		float3 D = (((HSV.z - RGB) / 6) + (C / 2)) / C;
		if (RGB.r == HSV.z)
			HSV.x = D.b - D.g;
		else if (RGB.g == HSV.z)
			HSV.x = (1.0 / 3.0) + D.r - D.b;
		else if (RGB.b == HSV.z)
			HSV.x = (2.0 / 3.0) + D.g - D.r;
		if (HSV.x < 0.0) { HSV.x += 1.0; }
		if (HSV.x > 1.0) { HSV.x -= 1.0; }
	}
	return HSV;
}

// Source: http://www.neilmendoza.com/glsl-rotation-about-an-arbitrary-axis/
float4x4 rotationMatrix(float3 axis, float angle)
{
	axis = normalize(axis);
	float s = sin(angle);
	float c = cos(angle);
	float oc = 1.0 - c;
	
	return float4x4(oc * axis.x * axis.x + c,           oc * axis.x * axis.y - axis.z * s,  oc * axis.z * axis.x + axis.y * s,  0.0,
				oc * axis.x * axis.y + axis.z * s,  oc * axis.y * axis.y + c,           oc * axis.y * axis.z - axis.x * s,  0.0,
				oc * axis.z * axis.x - axis.y * s,  oc * axis.y * axis.z + axis.x * s,  oc * axis.z * axis.z + c,           0.0,
				0.0,                                0.0,                                0.0,                                1.0);
}

float3 Rotate(float3 axis, float3 vec, float deg)
{
	return mul(rotationMatrix(axis, deg * (UNITY_PI / 180.0)), float4(vec, 0)).xyz;
}

#define TEMPLATE_FLT_2(functionName, a, b, body) \
float  functionName(float  a, float  b) { body; } \
float2 functionName(float2 a, float2 b) { body; } \
float3 functionName(float3 a, float3 b) { body; } \
float4 functionName(float4 a, float4 b) { body; }

#define TEMPLATE_FLT_3(functionName, a, b, c, body) \
float  functionName(float  a, float  b, float  c) { body; } \
float2 functionName(float2 a, float2 b, float2 c) { body; } \
float3 functionName(float3 a, float3 b, float3 c) { body; } \
float4 functionName(float4 a, float4 b, float4 c) { body; }

#define TEMPLATE_FLT_5(functionName, a, b, c, d, e, body) \
float  functionName(float  a, float  b, float  c, float  d, float  e) { body; } \
float2 functionName(float2 a, float2 b, float2 c, float2 d, float2 e) { body; } \
float3 functionName(float3 a, float3 b, float3 c, float3 d, float3 e) { body; } \
float4 functionName(float4 a, float4 b, float4 c, float4 d, float4 e) { body; }

float4 Remap(float4 i, float4 inputMin, float4 inputMax, float4 outputMin, float4 outputMax) { return outputMin + (i - inputMin) * (outputMax - outputMin) / (inputMax - inputMin); }
float3 Remap(float3 i, float3 inputMin, float3 inputMax, float3 outputMin, float3 outputMax) { return outputMin + (i - inputMin) * (outputMax - outputMin) / (inputMax - inputMin); }
float2 Remap(float2 i, float2 inputMin, float2 inputMax, float2 outputMin, float2 outputMax) { return outputMin + (i - inputMin) * (outputMax - outputMin) / (inputMax - inputMin); }
float Remap(float i, float inputMin, float inputMax, float outputMin, float outputMax) { return outputMin + (i - inputMin) * (outputMax - outputMin) / (inputMax - inputMin); }

// Clamp function that can invert min and max if min is greater than max
TEMPLATE_FLT_3(SmartClamp, x, a, b, if (any(a > b))	Swap(b, a); return clamp(x, a, b); )

TEMPLATE_FLT_5(RemapClamp, i, inputMin, intputMax, outputMin, outputMax, return SmartClamp(Remap(i, inputMin, intputMax, outputMin, outputMax), outputMin, outputMax))

float3 ScaleBias(float3 uv, float3 scale, float3 bias)
{
#ifdef CRT_CUBE
    return uv; // TODO
#else
    return (uv * scale) + bias;
#endif
}

float4 ScaleBias(float4 uv, float4 scale, float4 bias) { return float4(ScaleBias(uv.xyz, scale.xyz, bias.xyz), uv.a); }
float2 ScaleBias(float2 uv, float2 scale, float2 bias) { return ScaleBias(float3(uv.xy, 0), float3(scale.xy, 0), float3(bias.xy, 0)).xy; }

float Swizzle(float4 sourceValue, uint mode, float custom)
{
	switch (mode)
	{
	case 0: return sourceValue.x;
	case 1: return sourceValue.y;
	case 2: return sourceValue.z;
	case 3: return sourceValue.w;
	default:
	case 4: return 0.0f;
	case 5: return 0.5f;
	case 6: return 1.0f;
	case 7: return custom;
	}
	return 0;
}

#endif