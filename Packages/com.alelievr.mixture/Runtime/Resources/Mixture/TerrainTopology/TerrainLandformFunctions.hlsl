#ifndef __TERRAIN_LANDFORM_UTILS__
#define __TERRAIN_LANDFORM_UTILS__

#include "TerrainTopologyUtils.hlsl"
#include "Packages/com.alelievr.mixture/Runtime/Shaders/MixtureComputeUtils.hlsl"
#include "TerrainCurvatureFunctions.hlsl"


float GaussianLandform(float zx, float zy, float zxx, float zyy, float zxy)
{
    float K = GaussianCurvature(zx, zy, zxx, zyy, zxy);
    float H = MeanCurvature(zx, zy, zxx, zyy, zxy);

    //Hill (dome)
    if (K > 0 && H > 0)
        return 1.0;

    //Convex saddle
    if (K < 0 && H > 0)
        return 0.75f;

    //Perfect saddle, Antiform (perfect ridge), Synform (perfect valley), Plane.
    //Should be very rare.
    if (K == 0 || H == 0)
        return 0.5f;

    //Concave saddle
    if (K < 0 && H < 0)
        return 0.25f;

    //Depression (Basin)
    if (K > 0 && H < 0)
        return 0;

    return -1;
}

float ShapeIndexLandform(float zx, float zy, float zxx, float zyy, float zxy)
{
    float K = GaussianCurvature(zx, zy, zxx, zyy, zxy);
    float H = MeanCurvature(zx, zy, zxx, zyy, zxy);

    float d = SafeSqrt(H * H - K);

    float si = 2.0f / PI * atan(SafeDiv(H, d));

    return si * 0.5f + 0.5f;
}

float AccumulationLandform(float zx, float zy, float zxx, float zyy, float zxy)
{
    float Kh = HorizontalCurvature(zx, zy, zxx, zyy, zxy);
    float Kv = VerticalCurvature(zx, zy, zxx, zyy, zxy);

    //Dissipation flows.
    if (Kh > 0 && Kv > 0)
        return 1;

    //Convex transitive.
    if (Kh > 0 && Kv < 0)
        return 0.75f;

    //Planar transitive.
    //Should be very rare.
    if (Kh == 0 || Kv == 0)
        return 0.5f;

    //Concave trasitive.
    if (Kh < 0 && Kv > 0)
        return 0.25f;

    //Accumulative flows.
    if (Kh < 0 && Kv < 0)
        return 0;

    return -1;
}

float4 GetLandform(uint2 id)
{
    float2 d1;
    float3 d2;

    GetDerivatives(id, d1, d2);
    float landform = 0;
    float4 color = float4(1, 1, 1, 1);

    #if LANDFORM_GAUSSIAN
    landform = GaussianLandform(d1.x, d1.y, d2.x, d2.y, d2.z);
    color = Colorize(landform, 0, true);
    
    #elif LANDFORM_SHAPE_INDEX
    landform = ShapeIndexLandform(d1.x, d1.y, d2.x, d2.y, d2.z);
    color = Colorize(landform, 0, true);
    
    #elif LANDFORM_ACCUMULATION
    landform = AccumulationLandform(d1.x, d1.y, d2.x, d2.y, d2.z);
    color = Colorize(landform, 0, true);
    
    #endif
    
    return color;
}

#endif
