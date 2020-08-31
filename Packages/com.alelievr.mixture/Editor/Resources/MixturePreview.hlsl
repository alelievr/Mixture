#include "UnityCG.cginc"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/PhysicalCamera.hlsl"

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
    o.vertex = UnityObjectToClipPos(v.vertex);
    o.uv = v.uv;
    float3 screenUV = UnityObjectToViewPos(v.vertex);
    o.clipUV = mul(unity_GUIClipTextureMatrix, float4(screenUV, 1.0));
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

    // Then checkerboard
    imageColor.xyz = lerp(checkerboard, imageColor.xyz, imageColor.a);

    imageColor.xyz *= ConvertEV100ToExposure(_EV100);

    return imageColor * tex2D(_GUIClipTexture, i.clipUV).a;
}