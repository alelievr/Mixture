#ifndef FBM_NOISE
# define FBM_NOISE

#include "Packages/com.alelievr.mixture/Runtime/Shaders/PerlinNoise.hlsl"

// TODO: make the rotation a parameter
const float2x2 rMatrix = float2x2( 0.80,  0.60, -0.60,  0.80 );

#define FBM4_TEMPLATE(NAME, COORDINATE_TYPE, FUNC) \
float Generate##NAME##_FBM4(COORDINATE_TYPE p, float seed) \
{ \
    float f = 0.0; \
 \
    f += 0.5000 * (-1.0 + 2.0 * FUNC(p, seed)); p = mul(rMatrix, p) * 2.02; \
    f += 0.2500 * (-1.0 + 2.0 * FUNC(p, seed)); p = mul(rMatrix, p) * 2.03; \
    f += 0.1250 * (-1.0 + 2.0 * FUNC(p, seed)); p = mul(rMatrix, p) * 2.01; \
    f += 0.0625 * (-1.0 + 2.0 * FUNC(p, seed)); \
 \
    return f / 0.9375; \
} \
 \
float2 Generate##NAME##_FBM4_2(float2 p, float seed) \
{ \
    return float2( \
        Generate##NAME##_FBM4(p + -5.92, seed), \
        Generate##NAME##_FBM4(p + 3.88, seed) \
    ); \
}

#define FBM6_TEMPLATE(NAME, COORDINATE_TYPE, FUNC) \
float Generate##NAME##_FBM6(COORDINATE_TYPE p, float seed) \
{ \
    float f = 0.0; \
 \
    f += 0.500000 * FUNC(p, seed); p = mul(rMatrix, p) * 2.02; \
    f += 0.250000 * FUNC(p, seed); p = mul(rMatrix, p) * 2.03; \
    f += 0.125000 * FUNC(p, seed); p = mul(rMatrix, p) * 2.01; \
    f += 0.062500 * FUNC(p, seed); p = mul(rMatrix, p) * 2.04; \
    f += 0.031250 * FUNC(p, seed); p = mul(rMatrix, p) * 2.01; \
    f += 0.015625 * FUNC(p, seed); \
 \
    return f / 0.96875; \
} \
COORDINATE_TYPE Generate##NAME##_FBM6_2(COORDINATE_TYPE p, float seed) \
{ \
    return COORDINATE_TYPE( \
        Generate##NAME##_FBM6(p + 1.99, seed), \
        Generate##NAME##_FBM6(p + -4.18, seed) \
    ); \
}

float fbmPerlinNoise2D(float2 c, float seed) { return perlinNoise2D(c, seed).x; }

FBM4_TEMPLATE(Perlin2D, float2, fbmPerlinNoise2D);
FBM6_TEMPLATE(Perlin2D, float2, fbmPerlinNoise2D);

float GeneratePerlin2D_FBM(float2 coordinate, float seed)
{
    float2 uvSet1 = 0.5 * GeneratePerlin2D_FBM4_2(coordinate, seed) + 0.5;

    float2 n = GeneratePerlin2D_FBM6_2(4.0 * uvSet1, seed);

    float2 p = coordinate + 2.0*n;

    float f = 0.5 + 0.5 * GeneratePerlin2D_FBM4(2.0 * p, seed);

    return f;
}

#endif