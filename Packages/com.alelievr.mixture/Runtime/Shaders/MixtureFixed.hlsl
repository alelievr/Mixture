#ifndef MIXTURE_FIXED
#define MIXTURE_FIXED

#include "Packages/com.alelievr.mixture/Runtime/Shaders/MixtureUtils.hlsl"
#include "Packages/com.alelievr.mixture/Runtime/Shaders/CustomTexture.hlsl"

float3 GetDefaultUVs(v2f_customrendertexture i)
{
#ifdef CRT_CUBE
    return i.direction;
#elif CRT_2D
    return float3(i.localTexcoord.xy, 0.5);
#else
    return i.localTexcoord.xyz;
#endif
}

#endif
