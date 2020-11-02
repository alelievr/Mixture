#ifndef SPLATTER_HLSL
# define SPLATTER_HLSL

// Keep in sync with the buffer allocation in SplatterNode
struct SplatPoint
{
    float3 position;
    float3 rotation;
    float3 scale;
    uint id;
};

#endif