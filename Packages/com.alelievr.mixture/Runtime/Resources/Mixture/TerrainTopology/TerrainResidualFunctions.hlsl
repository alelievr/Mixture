#ifndef __TERRAIN_RESIDUAL_UTILS__
#define __TERRAIN_RESIDUAL_UTILS__

#include "TerrainTopologyUtils.hlsl"

float Mean(float data[7 * 7], int count)
{
    if (count == 0) return 0;

    float u = 0;
    for (int i = 0; i < (7 * 7); i++)
        u += data[i];

    return u / float(count);
}

float Variance(float data[7 * 7], float mean, int count)
{
    if (count == 0) return 0;

    float v = 0;
    for (int i = 0; i < (7 * 7) - 1; i++)
    {
        float diff = data[i] - mean;
        v += diff * diff;
    }

    return v / float(count);
}

float StdevElevation(float elevations[7 * 7], int count)
{
    float mean = Mean(elevations, count);
    return sqrt(Variance(elevations, mean, count));
}

float Percentile(float h, float elevations[7 * 7], int count)
{
    float num = 0;

    for (int i = 0; i < 7 * 7; i++)
        if (elevations[i] < h) num += 1.0;

    if (num == 0) return 0;
    if(count == 0) return 0;
    return num / float(count);
}

float4 GetResidual(uint2 id)
{
    float elevations[7 * 7];
    int loop = 0;
    int count = 0;
    for (int i = -3; i <= 3; i++)
    {
        for (int j = -3; j <= 3; j++)
        {
            int xi = (int)id.x + i;
            int yj = (int)id.y + j;
            if (xi < 0 || xi >= (int)_Heightmap.Length.x)
            {
                elevations[loop] = 0;
                loop++;
                continue;
            }
            if (yj < 0 || yj >= (int)_Heightmap.Length.y)
            {
                elevations[loop] = 0;
                loop++;
                continue;
            }
            float h = _Heightmap[uint2(xi, yj)];
            elevations[loop] = h;
            loop++;
            count++;
        }
    }

    float residual = 0;
    float h0 = _Heightmap[id.xy];
    float4 color = float4(1, 1, 1, 1);

    #if RESIDUAL_ELEVATION
    residual = h0;
    color = Colorize(residual, 0, true);
    
    #elif RESIDUAL_MEAN
    residual = Mean(elevations, count);
    color = Colorize(residual, 0, true);
    
    #elif RESIDUAL_DIFFERENCE
    residual = h0 - Mean(elevations, count);
    color = Colorize(residual, 4, false);
    
    #elif RESIDUAL_STDEV
    float o = StdevElevation(elevations, count);
    float d = h0 - Mean(elevations, count);
    residual = float(SafeDiv(d, o));
    color = Colorize(residual, 0.6, true);
    
    #elif RESIDUAL_DEVIATION
    float o = StdevElevation(elevations, count);
    float d = h0 - Mean(elevations, count);
    residual = float(SafeDiv(d, 0));
    color = Colorize(residual, 0.6, false);
    
    #elif RESIDUAL_PERCENTILE
    residual = Percentile(h0, elevations, count);
    color = Colorize(residual, 0.3, true);
    #endif

    return color;
}

#endif