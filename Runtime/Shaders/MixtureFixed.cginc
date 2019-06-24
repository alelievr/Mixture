// Mixture Fixed Pipeline helper


// Macros
#define MERGE_NAME(p1,p2) p1##p2

#define TEXELSIZE2D(Tex) MERGE_NAME(Tex,_TexelSize)
#define SAMPLER2D(Tex) MERGE_NAME(sampler,Tex)

#define TEXTURE2D(Tex) \
	Texture2D Tex; \
	SamplerState SAMPLER2D(Tex); \
	float4 TEXELSIZE2D(Tex);

#define SAMPLE2D(Texture, uv) Texture.Sample(SAMPLER2D(Texture), GetUV(uv,TEXELSIZE2D(Texture).y))
#define SAMPLE2D_S(Texture, Sampler, uv) Texture.Sample(Sampler, GetUV(uv,TEXELSIZE2D(Texture).y))

float2 GetUV(float2 uv, float sign)
{
	uv.y = 1 - uv.y;
	return uv;
}

// Inline Helopers
struct appdata
{
	float4 vertex : POSITION;
#ifdef USE_UV
	float2 uv : TEXCOORD0;
#endif
};

struct MixtureInputs
{
	float4 vertex : SV_POSITION;
#ifdef USE_UV
	float2 uv : TEXCOORD0;
#endif
};

MixtureInputs InitializeMixtureInputs(appdata v)
{
	MixtureInputs o;
	o.vertex = UnityObjectToClipPos(v.vertex);
	#ifdef USE_UV
		o.uv = v.uv;
	#endif
	return o;
}
 
#ifndef CUSTOM_VS
MixtureInputs vert(appdata v)
{
	return InitializeMixtureInputs(v);
}
#endif
