#ifndef MIXTURE_UTILS
#define MIXTURE_UTILS

float3 LatlongToDirectionCoordinate(float2 coord)
{
    float theta = coord.y * UNITY_PI;
    float phi = (coord.x * 2.f * UNITY_PI - UNITY_PI*0.5f);

    float cosTheta = cos(theta);
    float sinTheta = sqrt(1.0 - min(1.0, cosTheta*cosTheta));
    float cosPhi = cos(phi);
    float sinPhi = sin(phi);

    float3 direction = float3(sinTheta*cosPhi, cosTheta, sinTheta*sinPhi);
    direction.xy *= -1.0;
    return direction;
}

#endif