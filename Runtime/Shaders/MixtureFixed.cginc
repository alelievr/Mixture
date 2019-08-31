#ifndef MIXTURE_FIXED
#define MIXTURE_FIXED

// Mixture Fixed Pipeline helper

// Macros
#define MERGE_NAME(p1,p2) p1##p2

#define TEXELSIZE2D(Tex) MERGE_NAME(Tex,_TexelSize)
#define SAMPLER2D(Tex) MERGE_NAME(sampler,Tex)

#define TEXTURE2D(Tex) \
	sampler2D Tex;

#define TEXTURE3D(Tex) \
	sampler3D Tex;

#define TEXTURECUBE(Tex) \
	samplerCUBE Tex; \

#define SAMPLE2D(Texture, uv) Texture.Sample(SAMPLER2D(Texture), uv)
#define SAMPLE2D_S(Texture, Sampler, uv) Texture.Sample(Sampler, uv)
#define SAMPLE2D_LOD(Texture, uv, lod) Texture.Sample(SAMPLER2D(Texture), float4(uv.xy,0,lod))
#define SAMPLE2D_LOD_S(Texture, Sampler, uv) Texture.Sample(Sampler, float4(uv.xy,0,lod))

#ifdef CRT_2D
	#define SAMPLE_X(tex, uv, dir)	tex2Dlod(MERGE_NAME(tex,_2D), float4(uv, 0))
	#define TEXTURE_X(tex)			TEXTURE2D(MERGE_NAME(tex,_2D))
#elif CRT_3D
	#define SAMPLE_X(tex, uv, dir)	tex3Dlod(MERGE_NAME(tex,_3D), float4(uv, 0))
	#define TEXTURE_X(tex)			TEXTURE3D(MERGE_NAME(tex,_3D))
#else
	#define SAMPLE_X(tex, uv, dir)	texCUBElod(MERGE_NAME(tex,_Cube), float4(dir, 0))
	#define TEXTURE_X(tex)			TEXTURECUBE(MERGE_NAME(tex,_Cube))
#endif

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

