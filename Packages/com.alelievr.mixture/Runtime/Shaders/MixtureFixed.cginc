#ifndef MIXTURE_FIXED
#define MIXTURE_FIXED

#include "Packages/com.alelievr.mixture/Runtime/Shaders/MixtureUtils.cginc"
#include "Packages/com.alelievr.mixture/Runtime/Shaders/CustomTexture.hlsl"

float3 GetDefaultUVs(v2f_customrendertexture i)
{
#ifdef CRT_CUBE
    return i.direction;
#else
    return i.localTexcoord.xyz;
#endif
}

#endif
