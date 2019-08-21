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
