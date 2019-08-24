#ifndef CUSTOM_TEXTURE_GTRAPH
#define CUSTOM_TEXTURE_GTRAPH

float4 SRGBToLinear( float4 c ) { return c; }
float3 SRGBToLinear( float3 c ) { return c; }

bool IsGammaSpace()
{
#ifdef UNITY_COLORSPACE_GAMMA
    return true;
#else
    return false;
#endif
}

#endif // CUSTOM_TEXTURE_GTRAPH