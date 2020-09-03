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

float3 tiledCellularNoise2D(float2 coordinate, float2 period)
{
    float2 baseCell = floor(coordinate);

    //first pass to find the closest cell
    float minDistToCell = 10;
    float2 toClosestCell;
    float2 closestCell;
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
        }
    }

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

    float random = rand2dTo1d(closestCell);
    return float3(pow(abs(minDistToCell), 2.2), random, minEdgeDistance); // Gamma convertion
}

float3 tiledCellularNoise3D(float3 coordinate, float3 period)
{
    float3 baseCell = floor(coordinate);

    //first pass to find the closest cell
    float minDistToCell = 10;
    float3 toClosestCell;
    float3 closestCell;
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
            }
        }
    }

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

    float random = rand3dTo1d(closestCell);
    return float3(pow(minDistToCell, 2.2), random, minEdgeDistance);
}

float3 GenerateCellularNoise2D(float2 coordinate) { return tiledCellularNoise2D(coordinate, float2(100000, 100000)); }
float3 GenerateCellularNoise3D(float3 coordinate) { return tiledCellularNoise3D(coordinate, float3(100000, 100000, 100000)); }
float3 GenerateRidgedCellularNoise2D(float2 coordinate) { return tiledCellularNoise2D(coordinate, float2(100000, 100000)) * 2 - 1; }
float3 GenerateRidgedCellularNoise3D(float3 coordinate) { return tiledCellularNoise3D(coordinate, float3(100000, 100000, 100000)) * 2 - 1; }

float3 ridgedTiledCellularNoise2D(float2 coordinate, float2 period) { return tiledCellularNoise2D(coordinate, period) * 2 - 1; }
float3 ridgedTiledCellularNoise3D(float3 coordinate, float3 period) { return tiledCellularNoise3D(coordinate, period) * 2 - 1; }

#ifdef _TILINGMODE_TILED

NOISE_TEMPLATE(Cellular2D, float2, float3, tiledCellularNoise2D(coordinate * frequency, frequency));
NOISE_TEMPLATE(Cellular3D, float3, float3, tiledCellularNoise3D(coordinate * frequency, frequency));
RIDGED_NOISE_TEMPLATE(Cellular2D, float2, float3, ridgedTiledCellularNoise2D(coordinate * frequency, frequency));
RIDGED_NOISE_TEMPLATE(Cellular3D, float3, float3, ridgedTiledCellularNoise3D(coordinate * frequency, frequency));

#else

NOISE_TEMPLATE(Cellular2D, float2, float3, GenerateCellularNoise2D(coordinate * frequency));
NOISE_TEMPLATE(Cellular3D, float3, float3, GenerateCellularNoise3D(coordinate * frequency));
RIDGED_NOISE_TEMPLATE(Cellular2D, float2, float3, GenerateRidgedCellularNoise2D(coordinate * frequency));
RIDGED_NOISE_TEMPLATE(Cellular3D, float3, float3, GenerateRidgedCellularNoise3D(coordinate * frequency));

#endif

// TODO
// CURL_NOISE_2D_TEMPLATE(Cellular2D, GenerateCellularNoise2D);
// CURL_NOISE_3D_TEMPLATE(Cellular3D, GenerateCellularNoise2D);

#endif