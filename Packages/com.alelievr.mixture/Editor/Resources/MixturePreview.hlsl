#include "Packages/com.alelievr.mixture/Runtime/Shaders/MixtureUtils.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/PhysicalCamera.hlsl"
#include "Packages/com.alelievr.mixture/Runtime/Shaders/MixtureSRGB.hlsl"
#include "Packages/com.alelievr.mixture/Runtime/Shaders/NoiseUtils.hlsl"

struct appdata
{
    float4 vertex : POSITION;
    float2 uv : TEXCOORD0;
};

struct v2f
{
    float2 uv : TEXCOORD0;
    float4 vertex : SV_POSITION;
    float2 clipUV : TEXCOORD1;
};

float4 _Channels;
float _PreviewMip;
float _SRGB;
float _EV100;
uniform float4x4 unity_GUIClipTextureMatrix;
sampler2D _GUIClipTexture;

v2f vert (appdata v)
{
    v2f o;
    o.vertex = UnityObjectToClipPos(v.vertex.xyz);
    o.uv = v.uv;
    float3 screenUV = UnityObjectToViewPos(v.vertex.xyz);
    o.clipUV = mul(unity_GUIClipTextureMatrix, float4(screenUV, 1.0)).xy;
    return o;
}

float4 MakePreviewColor(v2f i, float2 texelSize, float4 imageColor)
{
    float2 checkerboardUVs = ceil(fmod(i.uv * texelSize / 64.0, 1.0)-0.5);
    float3 checkerboard = lerp(0.3,0.4, checkerboardUVs.x != checkerboardUVs.y ? 1 : 0);

    if (_Channels.a == 0.0) 
        imageColor.a = 1.0;

    else if (_Channels.r == 0.0 && _Channels.g == 0.0 && _Channels.b == 0.0 && _Channels.a == 1.0)
    {
        imageColor.rgb = imageColor.a;
        imageColor.a = 1.0;
    }

    // Apply srgb convertion 
    imageColor.xyz = ConvertToSRGBIfNeeded(imageColor.xyz);
    // Then checkerboard
    imageColor.xyz = lerp(checkerboard, imageColor.xyz, imageColor.a);

    // Preview exposure offset
    imageColor.xyz *= pow(2, _EV100);

    return imageColor * tex2D(_GUIClipTexture, i.clipUV).a;
}

float4 RayMarchVolume(float3 ro, float3 rd, Texture3D volume, SamplerState samp, float mip, float startDistance = 0, float stopDistance = 1, float densityMultiplier = 1)
{
    float dist = 0;
    float4 accumulation = 0;
    const int quality = 150;
    const float step = rcp(quality);
    int stepCount = 0;

    ro += rd * startDistance;
    for (stepCount = 0; dist + startDistance < stopDistance; stepCount++)
    {
        float3 ray = (ro + rd * dist) * 0.5 + 0.5;
        float4 c = volume.SampleLevel(samp, ray, mip);
        c.a = max(c.a * densityMultiplier*densityMultiplier, FLT_MIN);
        c.rgb *= c.a;
        accumulation += c * (1 - accumulation.a);
        dist += step;

        if (accumulation.a >= 1 || stepCount > 1024)
            break;
    }
    
    return accumulation;
}

float4 RayMarchSDF(float3 ro, float3 rd, Texture3D volume, SamplerState samp, float mip, float startDistance = 0, float stopDistance = 1, float offset = 0, bool outputNormal = true)
{
    float dist = 0;
    float4 accumulation = 0;
    float alpha = 0;
    const int quality = 150;
    const float step = rcp(quality);
    const float2 epsylon = float2(step, 0);
    int stepCount = 0;

    ro += rd * startDistance;
    for (stepCount = 0; dist + startDistance < stopDistance; stepCount++)
    {
        float3 ray = (ro + rd * dist) * 0.5 + 0.5;
        float4 c = volume.SampleLevel(samp, ray , mip);
        if (c.r + offset < 0.0)
        {
            if (outputNormal)
            {
                // show normal:
                float3 normal = normalize(float3(
                    volume.SampleLevel(samp, ray + epsylon.xyy, mip).x - volume.SampleLevel(samp, ray - epsylon.xyy, mip).x,
                    volume.SampleLevel(samp, ray + epsylon.yxy, mip).x - volume.SampleLevel(samp, ray - epsylon.yxy, mip).x,
                    volume.SampleLevel(samp, ray + epsylon.yyx, mip).x - volume.SampleLevel(samp, ray - epsylon.yyx, mip).x
                ));
    
                return float4(normal * 0.5 + 0.5, 1);
            }
            else
                return c;
        }
        dist += step;

        if (accumulation.a >= 1 || stepCount > 1024)
            break;
    }
    
    return 0;
}

// Ray origin is "ro", ray direction is "rd"
// Returns "t" along the ray of min,max intersection, or (-1,-1) if no intersections are found.
// https://iquilezles.org/www/articles/intersectors/intersectors.htm
float2 RayBoxIntersection(float3 ro, float3 rd, float3 boxSize)
{
    float3 m = 1.0/rd;
    float3 n = m*ro;
    float3 k = abs(m)*boxSize;
    float3 t1 = -n - k;
    float3 t2 = -n + k;
    float tN = max(max(t1.x, t1.y), t1.z);
    float tF = min(min(t2.x, t2.y), t2.z);
    if (tN > tF || tF < 0.0)
        return -1; // no intersection
    else
        return float2(tN, tF);
}

// sphere of size ra centered at point ce
float2 RaySphereIntersection( in float3 ro, in float3 rd, in float3 ce, float ra )
{
    float3 oc = ro - ce;
    float b = dot( oc, rd );
    float c = dot( oc, oc ) - ra*ra;
    float h = b*b - c;
    if( h<0.0 ) return -1.0; // no intersection
    h = sqrt( h );
    return float2( -b-h, -b+h );
}
