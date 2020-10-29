#ifndef SPLATTER_HLSL
# define SPLATTER_HLSL

struct SplatPoint
{
    float3 position;
    float3 rotation;
    float3 scale;
    uint id;
};

#endif