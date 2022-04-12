#ifndef __TERRAIN_CURVATURE_UTILS__
#define __TERRAIN_CURVATURE_UTILS__

#include "TerrainTopologyUtils.hlsl"
#include "Packages/com.alelievr.mixture/Runtime/Shaders/MixtureComputeUtils.hlsl"

float PlanCurvature(float zx, float zy, float zxx, float zyy, float zxy)
{
    float zx2 = zx * zx;
    float zy2 = zy * zy;
    float p = zx2 + zy2;

    float n = zy2 * zxx - 2.0f * zxy * zx * zy + zx2 * zyy;
    float d = pow(p, 1.5f);
    return SafeDiv(n, d);
}


float HorizontalCurvature(float zx, float zy, float zxx, float zyy, float zxy)
{
    float zx2 = zx * zx;
    float zy2 = zy * zy;
    float p = zx2 + zy2;

    float n = zy2 * zxx - 2.0f * zxy * zx * zy + zx2 * zyy;
    float d = p * pow(p + 1, 0.5f);

    return SafeDiv(n, d);
}

float VerticalCurvature(float zx, float zy, float zxx, float zyy, float zxy)
{
    float zx2 = zx * zx;
    float zy2 = zy * zy;
    float p = zx2 + zy2;

    float n = zx2 * zxx + 2.0f * zxy * zx * zy + zy2 * zyy;
    float d = p * pow(p + 1, 1.5f);

    return SafeDiv(n, d);
}

float MeanCurvature(float zx, float zy, float zxx, float zyy, float zxy)
{
    float zx2 = zx * zx;
    float zy2 = zy * zy;
    float p = zx2 + zy2;

    float n = (1 + zy2) * zxx - 2.0f * zxy * zx * zy + (1 + zx2) * zyy;
    float d = 2 * pow(p + 1, 1.5f);

    return SafeDiv(n, d);
}

float GaussianCurvature(float zx, float zy, float zxx, float zyy, float zxy)
{
    float zx2 = zx * zx;
    float zy2 = zy * zy;
    float p = zx2 + zy2;

    float n = zxx * zyy - zxy * zxy;
    float d = pow(p + 1, 2.0);

    return SafeDiv(n, d);
}

float MinimalCurvature(float zx, float zy, float zxx, float zyy, float zxy)
{
    float H = MeanCurvature(zx, zy, zxx, zyy, zxy);
    float K = GaussianCurvature(zx, zy, zxx, zyy, zxy);

    return H - SafeSqrt(H * H - K);
}

float MaximalCurvature(float zx, float zy, float zxx, float zyy, float zxy)
{
    float H = MeanCurvature(zx, zy, zxx, zyy, zxy);
    float K = GaussianCurvature(zx, zy, zxx, zyy, zxy);

    return H + SafeSqrt(H * H - K);
}

float UnsphericityCurvature(float zx, float zy, float zxx, float zyy, float zxy)
{
    float H = MeanCurvature(zx, zy, zxx, zyy, zxy);
    float K = GaussianCurvature(zx, zy, zxx, zyy, zxy);

    return SafeSqrt(H * H - K);
}

float RotorCurvature(float zx, float zy, float zxx, float zyy, float zxy)
{
    float zx2 = zx * zx;
    float zy2 = zy * zy;
    float p = zx2 + zy2;

    float n = (zx2 - zy2) * zxy - zx * zy * (zxx - zyy);
    float d = pow(p, 1.5f);

    return SafeDiv(n, d);
}

float DifferenceCurvature(float zx, float zy, float zxx, float zyy, float zxy)
{
    float Kv = VerticalCurvature(zx, zy, zxx, zyy, zxy);
    float Kh = HorizontalCurvature(zx, zy, zxx, zyy, zxy);

    return 0.5f * (Kv - Kh);
}

float HorizontalExcessCurvature(float zx, float zy, float zxx, float zyy, float zxy)
{
    float Kh = HorizontalCurvature(zx, zy, zxx, zyy, zxy);
    float Kmin = MinimalCurvature(zx, zy, zxx, zyy, zxy);

    return Kh - Kmin;
}

float VerticalExcessCurvature(float zx, float zy, float zxx, float zyy, float zxy)
{
    float Kv = VerticalCurvature(zx, zy, zxx, zyy, zxy);
    float Kmin = MinimalCurvature(zx, zy, zxx, zyy, zxy);

    return Kv - Kmin;
}

float RingCurvature(float zx, float zy, float zxx, float zyy, float zxy)
{
    float H = MeanCurvature(zx, zy, zxx, zyy, zxy);
    float K = GaussianCurvature(zx, zy, zxx, zyy, zxy);
    float Kh = HorizontalCurvature(zx, zy, zxx, zyy, zxy);

    return 2 * H * Kh - Kh * Kh - K;
}

float AccumulationCurvature(float zx, float zy, float zxx, float zyy, float zxy)
{
    float Kh = HorizontalCurvature(zx, zy, zxx, zyy, zxy);
    float Kv = VerticalCurvature(zx, zy, zxx, zyy, zxy);

    return Kh * Kv;
}

float4 ComputeCurvature(uint2 id)
{
    float2 d1;
    float3 d2;

    GetDerivatives(id.x, id.y, d1, d2);

    float curvature = 0;
    float4 color = float4(1, 1, 1, 1);
    #ifdef CURVATURE_PLAN
    curvature = PlanCurvature(d1.x, d1.y, d2.x, d2.y, d2.z);
    color = Colorize(curvature, 1.5, false);
    #elif CURVATURE_HORIZONTAL
    curvature = HorizontalCurvature(d1.x, d1.y, d2.x, d2.y, d2.z);
    color = Colorize(curvature, 2.0, false);
    #elif CURVATURE_VERTICAL
    curvature = VerticalCurvature(d1.x, d1.y, d2.x, d2.y, d2.z);
    color = Colorize(curvature, 2.0, false);
    #elif CURVATURE_MEAN
    curvature = MeanCurvature(d1.x, d1.y, d2.x, d2.y, d2.z);
    color = Colorize(curvature, 2.4, false);
    #elif CURVATURE_GAUSSIAN
    curvature = GaussianCurvature(d1.x, d1.y, d2.x, d2.y, d2.z);
    color = Colorize(curvature, 5.0, false);
    #elif CURVATURE_MINIMAL
    curvature = MinimalCurvature(d1.x, d1.y, d2.x, d2.y, d2.z);
    color = Colorize(curvature, 2.5, false);
    #elif CURVATURE_MAXIMAL
    curvature = MaximalCurvature(d1.x, d1.y, d2.x, d2.y, d2.z);
    color = Colorize(curvature, 2.5, false);
    #elif CURVATURE_UNSPHERICITY
    curvature = UnsphericityCurvature(d1.x, d1.y, d2.x, d2.y, d2.z);
    color = Colorize(curvature, 2.0, true);
    #elif CURVATURE_ROTOR
    curvature = RotorCurvature(d1.x, d1.y, d2.x, d2.y, d2.z);
    color = Colorize(curvature, 2.5, false);
    #elif CURVATURE_DIFFERENCE
    curvature = DifferenceCurvature(d1.x, d1.y, d2.x, d2.y, d2.z);
    color = Colorize(curvature, 2.0, false);
    #elif CURVATURE_HORIZONTAL_EXCESS
    curvature = HorizontalExcessCurvature(d1.x, d1.y, d2.x, d2.y, d2.z);
    color = Colorize(curvature, 2.0, true);
    #elif CURVATURE_VERTICAL_EXCESS
    curvature = VerticalExcessCurvature(d1.x, d1.y, d2.x, d2.y, d2.z);
    color = Colorize(curvature, 2.0, true);
    #elif CURVATURE_RING
    curvature = RingCurvature(d1.x, d1.y, d2.x, d2.y, d2.z);
    color = Colorize(curvature, 5.0, true);
    #elif CURVATURE_ACCUMULATION
    curvature = AccumulationCurvature(d1.x, d1.y, d2.x, d2.y, d2.z);
    color = Colorize(curvature, 5.0, false);
    #endif
    // Insert your code here
    return color;
}

#endif
