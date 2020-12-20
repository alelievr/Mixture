#ifndef MIXTURE_FIXED
#define MIXTURE_FIXED

#include "Packages/com.alelievr.mixture/Runtime/Shaders/MixtureUtils.cginc"
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

float3 GetDefaultUVsComputeShader(uint3 id, float3 rcpTextureSize)
{
#ifdef CRT_CUBE
    return ComputeCubemapDirectionFromUV(id.xy * rcpTextureSize.xy, id.z);
#elif CRT_2D
    return float3(id.xy * rcpTextureSize.xy, 0.5);
#else
    return id * rcpTextureSize;
#endif
}

#endif
