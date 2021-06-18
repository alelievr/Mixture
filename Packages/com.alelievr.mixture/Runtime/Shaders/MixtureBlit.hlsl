#ifndef MIXTURE_BLIT
# define MIXTURE_BLIT

#include "Packages/com.alelievr.mixture/Runtime/Shaders/MixtureUtils.hlsl"
#include "Packages/com.alelievr.mixture/Runtime/Shaders/Blending.hlsl"

float2 _DestinationOffset;
float2 _DestinationScale;


struct Attributes
{
    uint vertexID : SV_VertexID;
};

struct Varyings
{
    float4 positionCS : SV_POSITION;
    float2 uv : TEXCOORD0;
};

float2 GetQuadTexCoordFlipLess(uint vertexID)
{
    uint topBit = vertexID >> 1;
    uint botBit = (vertexID & 1);
    float u = topBit;
    float v = (topBit + botBit) & 1; // produces 0 for indices 0,3 and 1 for 1,2
    return float2(u, v);
}

Varyings BlitVertexShader(Attributes input)
{
    Varyings output;
    output.positionCS = GetQuadVertexPosition(input.vertexID) * float4(_DestinationScale.x, _DestinationScale.y, 1, 1) + float4(_DestinationOffset.x, _DestinationOffset.y, 0, 0);
    output.positionCS.xy = output.positionCS.xy * 2 - 1;
    output.uv = GetQuadTexCoordFlipLess(input.vertexID);

    return output;
}

#endif // MIXTURE_BLIT