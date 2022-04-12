#ifndef __TERRAIN_FLOW_UTILS__
#define __TERRAIN_FLOW_UTILS__

#include "TerrainTopologyUtils.hlsl"

RWTexture2D<float> _WaterMap;
RWTexture3D<float> _OutFlow;
RWTexture2D<float4> _VectorField;

void ComputeOutFlow(int x, int y)
{
    int width = _Heightmap.Length.x;
    int height = _Heightmap.Length.y;

    int xn1 = (x == 0) ? 0 : x - 1;
    int xp1 = (x == width - 1) ? width - 1 : x + 1;
    int yn1 = (y == 0) ? 0 : y - 1;
    int yp1 = (y == height - 1) ? height - 1 : y + 1;

    float waterHt = _WaterMap[uint2(x, y)];
    float waterHts0 = _WaterMap[uint2(xn1, y)];
    float waterHts1 = _WaterMap[uint2(xp1, y)];
    float waterHts2 = _WaterMap[uint2(x, yn1)];
    float waterHts3 = _WaterMap[uint2(x, yp1)];

    float landHt = GetNormalizedHeight(x, y);
    float landHts0 = GetNormalizedHeight(xn1, y);
    float landHts1 = GetNormalizedHeight(xp1, y);
    float landHts2 = GetNormalizedHeight(x, yn1);
    float landHts3 = GetNormalizedHeight(x, yp1);

    float diff0 = (waterHt + landHt) - (waterHts0 + landHts0);
    float diff1 = (waterHt + landHt) - (waterHts1 + landHts1);
    float diff2 = (waterHt + landHt) - (waterHts2 + landHts2);
    float diff3 = (waterHt + landHt) - (waterHts3 + landHts3);

    //out flow is previous flow plus flow for this time step.
    float flow0 = max(0, _OutFlow[uint3(x, y, 0)] + diff0);
    float flow1 = max(0, _OutFlow[uint3(x, y, 1)] + diff1);
    float flow2 = max(0, _OutFlow[uint3(x, y, 2)] + diff2);
    float flow3 = max(0, _OutFlow[uint3(x, y, 3)] + diff3);

    float sum = flow0 + flow1 + flow2 + flow3;

    if (sum > 0.0f)
    {
        //If the sum of the outflow flux exceeds the amount in the cell
        //flow value will be scaled down by a factor K to avoid negative update.
        const float TIME = 0.2;
        float K = waterHt / (sum * TIME);
        if (K > 1.0f) K = 1.0f;
        if (K < 0.0f) K = 0.0f;

        _OutFlow[uint3(x, y, 0)] = flow0 * K;
        _OutFlow[uint3(x, y, 1)] = flow1 * K;
        _OutFlow[uint3(x, y, 2)] = flow2 * K;
        _OutFlow[uint3(x, y, 3)] = flow3 * K;
        float4 right = float4(1, 0, 0, 1);
        float4 left = float4(0, 0, 0, 1);
        float4 top = float4(0, 1, 0, 1);
        float4 bottom = float4(0, 0, 0, 1);
        float4 vectorField = (
            left * flow0 + right * flow1 + bottom * flow2 + top * flow3
        );
        _VectorField[uint2(x, y)] += vectorField;
    }
    else
    {
        _OutFlow[uint3(x, y, 0)] = 0.0f;
        _OutFlow[uint3(x, y, 1)] = 0.0f;
        _OutFlow[uint3(x, y, 2)] = 0.0f;
        _OutFlow[uint3(x, y, 3)] = 0.0f;
       // _VectorField[uint2(x, y)] = float4(0, 0, 0, 1);
       // _VectorField[uint2(x, y)] = float4(0, 0, 0, 1);
       // _VectorField[uint2(x, y)] = float4(0, 0, 0, 1);
       // _VectorField[uint2(x, y)] = float4(0, 0, 0, 1);
    }
    
}

void UpdateWaterMap(int x, int y)
{
    const int LEFT = 0;
    const int RIGHT = 1;
    const int BOTTOM = 2;
    const int TOP = 3;
    const int width = _Heightmap.Length.x;
    const int height = _Heightmap.Length.y;
    const float TIME = 0.2;
    float flowOUT = _OutFlow[uint3(x, y, 0)] + _OutFlow[uint3(x, y, 1)] + _OutFlow[uint3(x, y, 2)] + _OutFlow[
        uint3(x, y, 3)];
    float flowIN = 0.0f;

    //Flow in is inflow from neighour cells. Note for the cell on the left you need 
    //thats cells flow to the right (ie it flows into this cell)
    flowIN += (x == 0) ? 0.0f : _OutFlow[uint3(x - 1, y, RIGHT)];
    flowIN += (x == width - 1) ? 0.0f : _OutFlow[uint3(x + 1, y, LEFT)];
    flowIN += (y == 0) ? 0.0f : _OutFlow[uint3(x, y - 1, TOP)];
    flowIN += (y == height - 1) ? 0.0f : _OutFlow[uint3(x, y + 1, BOTTOM)];

    float ht = _WaterMap[uint2(x, y)] + (flowIN - flowOUT) * TIME;
    if (ht < 0.0f) ht = 0.0f;

    //Result is net volume change over time
    _WaterMap[uint2(x, y)] = ht;
}

float4 CalculateVelocityField(int x, int y)
{
    const int LEFT = 0;
    const int RIGHT = 1;
    const int BOTTOM = 2;
    const int TOP = 3;
    const int width = _Heightmap.Length.x;
    const int height = _Heightmap.Length.y;
    const float TIME = 0.2;

    float dl = (x == 0) ? 0.0f : _OutFlow[uint3(x - 1, y, RIGHT)] - _OutFlow[uint3(x, y, LEFT)];

    float dr = (x == width - 1) ? 0.0f : _OutFlow[uint3(x, y, RIGHT)] - _OutFlow[uint3(x + 1, y, LEFT)];

    float dt = (y == height - 1) ? 0.0f : _OutFlow[uint3(x, y + 1, BOTTOM)] - _OutFlow[uint3(x, y, TOP)];

    float db = (y == 0) ? 0.0f : _OutFlow[uint3(x, y, BOTTOM)] - _OutFlow[uint3(x, y - 1, TOP)];

    float vx = (dl + dr) * 0.5f;
    float vy = (db + dt) * 0.5f;

    float flow = sqrt(vx * vx + vy * vy) * 100;
    float3 nFlow = normalize(float3(flow, flow, flow));
    float4 v = _VectorField[uint2(x, y)];
    float4 d = float4(0.5, .5, 0, 1);
    _VectorField[uint2(x, y)] = d + (v * flow * 500);
    return float4(flow, flow, flow, 1);
}

#endif