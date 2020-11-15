#ifndef CELLULAR_NOISE
# define CELLULAR_NOISE

#include "Packages/com.alelievr.mixture/Runtime/Shaders/NoiseUtils.hlsl"

// Convert a 0.0->1.0 sample to a -1.0->1.0 sample weighted towards the extremes
float4 CellularWeightSamples(float4 samples)
{
    samples = samples * 2.0f - 1.0f;
    //return (1.0 - samples * samples) * sign(samples);	// square
    return (samples * samples * samples) - sign(samples);	// cubic (even more variance)
}

// Credits: https://www.ronja-tutorials.com/2018/10/06/tiling-noise.html
float rand3dTo1d(float3 value, float3 dotDir = float3(12.9898, 78.233, 37.719))
{
	//make value smaller to avoid artefacts
	float3 smallValue = sin(value);
	//get scalar value from 3d vector
	float random = dot(smallValue, dotDir);
	//make value more random by making it bigger and then taking the factional part
	random = frac(sin(random) * 143758.5453);
	return random;
}

float3 rand3dTo3d(float3 value)
{
	return float3(
		rand3dTo1d(value, float3(12.989, 78.233, 37.719)),
		rand3dTo1d(value, float3(39.346, 11.135, 83.155)),
		rand3dTo1d(value, float3(73.156, 52.235, 09.151))
	);
}

float rand2dTo1d(float2 value, float2 dotDir = float2(12.9898, 78.233))
{
	float2 smallValue = sin(value);
	float random = dot(smallValue, dotDir);
	random = frac(sin(random) * 143758.5453);
	return random;
}

float2 rand2dTo2d(float2 value)
{
	return float2(
		rand2dTo1d(value, float2(12.989, 78.233)),
		rand2dTo1d(value, float2(39.346, 11.135))
	);
}

float2 modulo(float2 divident, float2 divisor)
{
    float2 positiveDivident = divident % divisor + divisor;
    return positiveDivident % divisor;
}

float3 modulo(float3 divident, float3 divisor)
{
    float3 positiveDivident = divident % divisor + divisor;
    return positiveDivident % divisor;
}

float4 tiledCellularNoise2D(float2 coordinate, float2 period, float seed)
{
    float2 baseCell = floor(coordinate);

    //first pass to find the closest cell
    float minDistToCell = 10;
    float2 toClosestCell;
    float2 closestCell;
    float smoothDistance = 0;
    for(int x1=-1; x1<=1; x1++)
    {
        for(int y1=-1; y1<=1; y1++)
        {
            float2 cell = baseCell + float2(x1, y1);
            float2 tiledCell = modulo(cell, period);
            float2 cellPosition = cell + rand2dTo2d(tiledCell);
            float2 toCell = cellPosition - coordinate;
            float distToCell = Distance(toCell);

            if(distToCell < minDistToCell)
            {
                minDistToCell = distToCell;
                closestCell = cell;
                toClosestCell = toCell;
            }

            smoothDistance += exp(-16.0 * distToCell);
        }
    }
    // Source: https://iquilezles.org/www/articles/smoothvoronoi/smoothvoronoi.htm
    smoothDistance = -(1 / 16.0) * log(smoothDistance);

    //second pass to find the distance to the closest edge
    float minEdgeDistance = 10;
    for(int x2=-1; x2<=1; x2++)
    {
        for(int y2=-1; y2<=1; y2++)
        {
            float2 cell = baseCell + float2(x2, y2);
            float2 tiledCell = modulo(cell, period);
            float2 cellPosition = cell + rand2dTo2d(tiledCell);
            float2 toCell = cellPosition - coordinate;

            float2 diffToClosestCell = abs(closestCell - cell);
            bool isClosestCell = diffToClosestCell.x + diffToClosestCell.y < 0.1;
            if(!isClosestCell)
            {
                float2 toCenter = (toClosestCell + toCell) * 0.5;
                float2 cellDifference = normalize(toCell - toClosestCell);
                float edgeDistance = dot(toCenter, cellDifference);
                minEdgeDistance = min(minEdgeDistance, edgeDistance);
            }
        }
    }

    float random = rand2dTo1d(modulo(closestCell, period));
    return float4(pow(abs(minDistToCell), 2.2), random, minEdgeDistance, pow(abs(smoothDistance), 2.2)); // Gamma convertion
}

float4 tiledCellularNoise3D(float3 coordinate, float3 period, float seed)
{
    float3 baseCell = floor(coordinate);

    //first pass to find the closest cell
    float minDistToCell = 10;
    float3 toClosestCell;
    float3 closestCell;
    float smoothDistance = 0;
    for(int x1=-1; x1<=1; x1++)
    {
        for(int y1=-1; y1<=1; y1++)
        {
            for(int z1=-1; z1<=1; z1++)
            {
                float3 cell = baseCell + float3(x1, y1, z1);
                float3 tiledCell = modulo(cell, period);
                float3 cellPosition = cell + rand3dTo3d(tiledCell);
                float3 toCell = cellPosition - coordinate;
                float distToCell = Distance(toCell);
                if(distToCell < minDistToCell)
                {
                    minDistToCell = distToCell;
                    closestCell = cell;
                    toClosestCell = toCell;
                }
                smoothDistance += exp(-16.0 * distToCell);
            }
        }
    }
    // Source: https://iquilezles.org/www/articles/smoothvoronoi/smoothvoronoi.htm
    smoothDistance = -(1 / 16.0) * log(smoothDistance);

    //second pass to find the distance to the closest edge
    float minEdgeDistance = 10;
    for(int x2=-1; x2<=1; x2++)
    {
        for(int y2=-1; y2<=1; y2++)
        {
            for(int z2=-1; z2<=1; z2++)
            {
                float3 cell = baseCell + float3(x2, y2, z2);
                float3 tiledCell = modulo(cell, period);
                float3 cellPosition = cell + rand3dTo3d(tiledCell);
                float3 toCell = cellPosition - coordinate;

                float3 diffToClosestCell = abs(closestCell - cell);
                bool isClosestCell = diffToClosestCell.x + diffToClosestCell.y + diffToClosestCell.z < 0.1;
                if(!isClosestCell)
                {
                    float3 toCenter = (toClosestCell + toCell) * 0.5;
                    float3 cellDifference = normalize(toCell - toClosestCell);
                    float edgeDistance = dot(toCenter, cellDifference);
                    minEdgeDistance = min(minEdgeDistance, edgeDistance);
                }
            }
        }
    }

    float random = rand3dTo1d(modulo(closestCell, period));
    return float4(pow(abs(minDistToCell), 2.2), random, minEdgeDistance, pow(abs(smoothDistance), 2.2));
}

float4 GenerateCellularNoise2D(float2 coordinate, float seed) { return tiledCellularNoise2D(coordinate, float2(100000, 100000), seed); }
float4 GenerateCellularNoise3D(float3 coordinate, float seed) { return tiledCellularNoise3D(coordinate, float3(100000, 100000, 100000), seed); }
float4 GenerateRidgedCellularNoise2D(float2 coordinate, float seed) { return tiledCellularNoise2D(coordinate, float2(100000, 100000), seed) * 2 - 1; }
float4 GenerateRidgedCellularNoise3D(float3 coordinate, float seed) { return tiledCellularNoise3D(coordinate, float3(100000, 100000, 100000), seed) * 2 - 1; }

float4 ridgedTiledCellularNoise2D(float2 coordinate, float2 period, float seed) { return tiledCellularNoise2D(coordinate, period, seed) * 2 - 1; }
float4 ridgedTiledCellularNoise3D(float3 coordinate, float3 period, float seed) { return tiledCellularNoise3D(coordinate, period, seed) * 2 - 1; }

#ifdef _TILINGMODE_TILED

NOISE_TEMPLATE(Cellular2D, float2, float4, tiledCellularNoise2D(coordinate * frequency, frequency, seed));
NOISE_TEMPLATE(Cellular3D, float3, float4, tiledCellularNoise3D(coordinate * frequency, frequency, seed));
RIDGED_NOISE_TEMPLATE(Cellular2D, float2, float4, ridgedTiledCellularNoise2D(coordinate * frequency, frequency, seed));
RIDGED_NOISE_TEMPLATE(Cellular3D, float3, float4, ridgedTiledCellularNoise3D(coordinate * frequency, frequency, seed));

#else

NOISE_TEMPLATE(Cellular2D, float2, float4, GenerateCellularNoise2D(coordinate * frequency, seed));
NOISE_TEMPLATE(Cellular3D, float3, float4, GenerateCellularNoise3D(coordinate * frequency, seed));
RIDGED_NOISE_TEMPLATE(Cellular2D, float2, float4, GenerateRidgedCellularNoise2D(coordinate * frequency, seed));
RIDGED_NOISE_TEMPLATE(Cellular3D, float3, float4, GenerateRidgedCellularNoise3D(coordinate * frequency, seed));

#endif

// TODO
// CURL_NOISE_2D_TEMPLATE(Cellular2D, GenerateCellularNoise2D);
// CURL_NOISE_3D_TEMPLATE(Cellular3D, GenerateCellularNoise2D);

float4 GenerateCellularNoise(v2f_customrendertexture i, int seed);

float SwizzleCellMode(float4 noise, int mode)
{
    switch (mode)
    {
        default: // Distance To Cell
        case 0: return noise.x;
        case 1: return noise.y; // Cells
        case 2: return noise.z; // Valley
        case 3: return noise.w; // Smooth Distance To Cell
    }
}

float4 GenerateCellularNoiseForChannels(v2f_customrendertexture i, int seed)
{
    float4 noise = GenerateCellularNoise(i, seed);
    float4 color = float4(0, 0, 0, 1);
    color.r = SwizzleCellMode(noise, _CellsModeR);

    if (_Channels == 0) // RRRR
        color = color.rrrr;
    else
    {
        if (_Channels > 1) // G
        {
            if (_CellsModeG == _CellsModeR)
                noise = GenerateCellularNoise(i, seed + 42);
            color.g = SwizzleCellMode(noise, _CellsModeG);
        }
        if (_Channels > 2) // B
        {
            if (_CellsModeB == _CellsModeG || _CellsModeB == _CellsModeR)
                noise = GenerateCellularNoise(i, seed - 69);
            color.b = SwizzleCellMode(noise, _CellsModeB);
        }
        if (_Channels > 3) // A
        {
            if (_CellsModeA == _CellsModeB || _CellsModeA == _CellsModeG || _CellsModeA == _CellsModeR)
                noise = GenerateCellularNoise(i, seed + 5359);
            color.a = SwizzleCellMode(noise, _CellsModeA);
        }
    }

    return color;
} 

#endif