#ifndef MIXTURE_SRGB
# define MIXTURE_SRGB

int _IsSRGB;

#ifndef UNITY_COLOR_INCLUDED

float3 SRGBToLinear(float3 c)
{
    float3 linearRGBLo  = c / 12.92;
    float3 linearRGBHi  = pow(abs((c + 0.055) / 1.055), float3(2.4, 2.4, 2.4));
    float3 linearRGB    = (c <= 0.04045) ? linearRGBLo : linearRGBHi;
    return linearRGB;
}

float3 LinearToSRGB(float3 c)
{
    float3 sRGBLo = c * 12.92;
    float3 sRGBHi = (pow(abs(c), float3(1.0/2.4, 1.0/2.4, 1.0/2.4)) * 1.055) - 0.055;
    float3 sRGB   = (c <= 0.0031308) ? sRGBLo : sRGBHi;
    return sRGB;
}

#endif

float3 ConvertToSRGBIfNeeded(float3 color)
{
    if (_IsSRGB)
        return color;
    else
        return LinearToSRGB(color);
}

float3 ConvertToLinearIfNeeded(float3 color)
{
    if (_IsSRGB)
        return SRGBToLinear(color);
    else
        return color;
}


#endif