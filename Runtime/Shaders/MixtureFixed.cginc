#ifndef MIXTURE_FIXED
#define MIXTURE_FIXED

// Mixture Fixed Pipeline helper

// Utility samplers:
sampler s_linear_clamp_sampler;
sampler s_linear_repeat_sampler;
sampler s_point_clamp_sampler;
sampler s_point_repeat_sampler;

// Macros
#define MERGE_NAME(p1,p2) p1##p2

#define TEXELSIZE2D(Tex) MERGE_NAME(Tex,_TexelSize)
#define SAMPLER2D(Tex) MERGE_NAME(sampler,Tex)

#define TEXTURE2D(textureName)                Texture2D textureName
#define TEXTURE2D_ARRAY(textureName)          Texture2DArray textureName
#define TEXTURECUBE(textureName)              TextureCube textureName
#define TEXTURECUBE_ARRAY(textureName)        TextureCubeArray textureName
#define TEXTURE3D(textureName)                Texture3D textureName

#define SAMPLER(samplerName)                  SamplerState samplerName
#define SAMPLER_CMP(samplerName)              SamplerComparisonState samplerName

#define SAMPLE_TEXTURE2D(textureName, samplerName, coord2)                               textureName.Sample(samplerName, coord2)
#define SAMPLE_TEXTURE2D_LOD(textureName, samplerName, coord2, lod)                      textureName.SampleLevel(samplerName, coord2, lod)
#define SAMPLE_TEXTURECUBE(textureName, samplerName, coord3)                             textureName.Sample(samplerName, coord3)
#define SAMPLE_TEXTURECUBE_LOD(textureName, samplerName, coord3, lod)                    textureName.SampleLevel(samplerName, coord3, lod)
#define SAMPLE_TEXTURE3D(textureName, samplerName, coord3)                               textureName.Sample(samplerName, coord3)
#define SAMPLE_TEXTURE3D_LOD(textureName, samplerName, coord3, lod)                      textureName.SampleLevel(samplerName, coord3, lod)

#define LOAD_TEXTURE2D(textureName, unCoord2)                                   textureName.Load(int3(unCoord2, 0))
#define LOAD_TEXTURE2D_LOD(textureName, unCoord2, lod)                          textureName.Load(int3(unCoord2, lod))
#define LOAD_TEXTURE3D(textureName, unCoord3)                                   textureName.Load(int4(unCoord3, 0))
#define LOAD_TEXTURE3D_LOD(textureName, unCoord3, lod)                          textureName.Load(int4(unCoord3, lod))

#define TEXTURE_SAMPLER2D(name) sampler2D name
#define TEXTURE_SAMPLER3D(name) sampler3D name
#define TEXTURE_SAMPLERCUBE(name) samplerCUBE name

#ifdef CRT_2D
	#define SAMPLE_X(tex, uv, dir)	tex2Dlod(MERGE_NAME(tex,_2D), float4(uv, 0))
	#define SAMPLE_X_LINEAR_CLAMP(tex, uv, dir)	SAMPLE_TEXTURE2D_LOD(MERGE_NAME(tex,_2D), s_linear_clamp_sampler, uv.xy, 0)
	#define TEXTURE_SAMPLER_X(tex)	TEXTURE_SAMPLER2D(MERGE_NAME(tex,_2D))
	#define TEXTURE_X(name) TEXTURE2D(MERGE_NAME(name,_2D))
#elif CRT_3D
	#define SAMPLE_X(tex, uv, dir)	tex3Dlod(MERGE_NAME(tex,_3D), float4(uv, 0))
	#define SAMPLE_X_LINEAR_CLAMP(tex, uv, dir)	SAMPLE_TEXTURE3D_LOD(MERGE_NAME(tex,_3D), s_linear_clamp_sampler, uv.xyz, 0)
	#define TEXTURE_SAMPLER_X(tex)	TEXTURE_SAMPLER3D(MERGE_NAME(tex,_3D))
	#define TEXTURE_X(name) TEXTURE3D(MERGE_NAME(name,_3D))
#else
	#define SAMPLE_X(tex, uv, dir)	texCUBElod(MERGE_NAME(tex,_Cube), float4(dir, 0))
	#define SAMPLE_X_LINEAR_CLAMP(tex, uv, dir)	SAMPLE_TEXTURECUBE_LOD(MERGE_NAME(tex,_Cube), s_linear_clamp_sampler, dir, 0)
	#define TEXTURE_SAMPLER_X(tex)	TEXTURE_SAMPLERCUBE(MERGE_NAME(tex,_Cube))
	#define TEXTURE_X(name) TEXTURECUBE(MERGE_NAME(name,_Cube))
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

#endif
