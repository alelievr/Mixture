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

#endif
