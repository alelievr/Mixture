#ifndef SPLATTER_HLSL
# define SPLATTER_HLSL

#define MAX_DEPTH_VALUE 10000

// Keep in sync with the buffer allocation in SplatterNode
struct SplatPoint
{
    float3 position;
    float3 rotation;
    float3 scale;
    uint id;
};

#endif