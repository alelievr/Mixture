#ifndef __TERRAIN_TOPOLOGY_UTILS__
#define __TERRAIN_TOPOLOGY_UTILS__

#include "Packages/com.alelievr.mixture/Runtime/Shaders/MixtureComputeUtils.hlsl"

Texture2D<float> _Heightmap;
float _TerrainHeight;
float2 _CellSize;
int _UseRamp;
SamplerState linear_clamp_sampler;

Texture2D<float4> _Gradient;
Texture2D<float4> _PosGradient;
Texture2D<float4> _NegGradient;

#define PI 3.141592653589793
#define Deg2Rag 180.0 / PI

float2 GetUV(uint2 index)
{
    return float2(index) / float2(_Heightmap.Length.xy);
}

float GetNormalizedHeight(uint2 index)
{
    float2 uv = GetUV(index);
    return clamp(_Heightmap.SampleLevel(linear_clamp_sampler, uv, 0), 0.0, 1.0);
}

float GetNormalizedHeight(int x, int y)
{
    float2 uv = GetUV(uint2(x, y));
    return clamp(_Heightmap.SampleLevel(linear_clamp_sampler, uv, 0), 0.0, 1.0);
}

float GetHeight(uint2 index)
{
    return GetNormalizedHeight(index) * _TerrainHeight;
}

float GetHeight(int x, int y)
{
    return GetNormalizedHeight(uint2(x, y)) * _TerrainHeight;
}

float2 GetFirstDerivative(int x, int y)
{
    float w = _CellSize.x;
    float z1 = GetHeight(x - 1, y + 1);
    float z2 = GetHeight(x + 0, y + 1);
    float z3 = GetHeight(x + 1, y + 1);
    float z4 = GetHeight(x - 1, y + 0);
    float z6 = GetHeight(x + 1, y + 0);
    float z7 = GetHeight(x - 1, y - 1);
    float z8 = GetHeight(x + 0, y - 1);
    float z9 = GetHeight(x + 1, y - 1);


    float zx = (z3 + z6 + z9 - z1 - z4 - z7) / (6.0f * w);
    float zy = (z1 + z2 + z3 - z7 - z8 - z9) / (6.0f * w);

    return float2(-zx, -zy);
}

float2 GetFirstDerivative(uint2 index)
{
    return GetFirstDerivative(index.x, index.y);
}

void GetDerivatives(int x, int y, inout float2 d1, inout float3 d2)
{
    float w = _CellSize.x;
    float w2 = w * w;
    float z1 = GetHeight(x - 1, y + 1);
    float z2 = GetHeight(x + 0, y + 1);
    float z3 = GetHeight(x + 1, y + 1);
    float z4 = GetHeight(x - 1, y + 0);
    float z5 = GetHeight(x + 0, y + 0);
    float z6 = GetHeight(x + 1, y + 0);
    float z7 = GetHeight(x - 1, y - 1);
    float z8 = GetHeight(x + 0, y - 1);
    float z9 = GetHeight(x + 1, y - 1);

    //p, q
    float zx = (z3 + z6 + z9 - z1 - z4 - z7) / (6.0f * w);
    float zy = (z1 + z2 + z3 - z7 - z8 - z9) / (6.0f * w);

    //r, t, s
    float zxx = (z1 + z3 + z4 + z6 + z7 + z9 - 2.0f * (z2 + z5 + z8)) / (3.0f * w2);
    float zyy = (z1 + z2 + z3 + z7 + z8 + z9 - 2.0f * (z4 + z5 + z6)) / (3.0f * w2);
    float zxy = (z3 + z7 - z1 - z9) / (4.0f * w2);

    d1 = float2(-zx, -zy);
    d2 = float3(-zxx, -zyy, -zxy);
}

void GetDerivatives(uint2 index, inout float2 d1, inout float3 d2)
{
    GetDerivatives(index.x, index.y, d1, d2);
}

float remap(float value, float from1, float to1, float from2, float to2)
{
    return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
}

float4 Colorize(float v, float exponent, bool nonNegative)
{
    if (exponent > 0)
    {
        float s = sign(v);
        float p = pow(10, exponent);
        float l = log(1.0 + p * abs(v));

        v = s * l;
    }

    if (nonNegative)
    {
        return _Gradient.SampleLevel(linear_clamp_sampler, float2(v, 0), 0);
    }
    else
    {
        if (v > 0)
        {
            return _PosGradient.SampleLevel(linear_clamp_sampler, float2(v, 0), 0);
        }
        else
        {
            return _NegGradient.SampleLevel(linear_clamp_sampler, float2(-v, 0), 0);
        }
    }
}

float GetSlope(float zx, float zy)
{
    float p = zx * zx + zy * zy;
    float g = SafeSqrt(p); // TODO : Implement safe Sqrt ?

    return atan(g) * Deg2Rag / 90.0;
}

float GetAspect(float zx, float zy)
{
    float gyx = SafeDiv(zy, zx);
    float gxx = SafeDiv(zx, abs(zx));
    float aspect = 180 - atan(gyx) * Deg2Rag + 90 * gxx;
    aspect /= 360;
    return aspect;
}

float4 GetNormal(float2 d1)
{
    float3 n;
    n.x = d1.x * 0.5 + 0.5;
    n.y = -d1.y * 0.5 + 0.5;
    n.z = 1.0;
    n = normalize(n);
    return float4(n, 1);
}

#endif
