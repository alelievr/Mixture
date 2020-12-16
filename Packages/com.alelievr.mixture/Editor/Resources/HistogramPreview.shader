Shader "Hidden/HistogramPreview"
{
    Properties
    {
    }
	SubShader
    {
        Tags { "RenderType"="Overlay" }
        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off
        ZTest LEqual

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 5.0

            #pragma shader_feature CRT_2D CRT_3D CRT_CUBE
            #pragma enable_d3d11_debug_symbols

            #include "Packages/com.alelievr.mixture/Runtime/Shaders/MixtureUtils.cginc"
            // #include "Packages/com.alelievr.mixture/Editor/Resources/HistogramData.hlsl"

            // Keep in sync with HistogramView.cs buffer alloc
struct LuminanceData
{
    float minLuminance;
    float maxLuminance;
};

// Keep in sync with HistogramView.cs buffer alloc
struct HistogramData
{
    uint minBucketCount;
    uint maxBucketCount;
};

ByteAddressBuffer                 _Histogram2;
uint                                _HistogramBucketCount;
StructuredBuffer<LuminanceData>   _ImageLuminance;
StructuredBuffer<HistogramData>   _HistogramData;


            // Unity UI
            uniform float4x4 unity_GUIClipTextureMatrix;
            sampler2D _GUIClipTexture;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 clipUV : TEXCOORD1;
            };

            inline float3 UnityObjectToViewPos( in float3 pos )
            {
                return mul(UNITY_MATRIX_V, mul(unity_ObjectToWorld, float4(pos, 1.0))).xyz;
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                float3 screenUV = UnityObjectToViewPos(v.vertex.xyz).xyz;
                o.clipUV = mul(unity_GUIClipTextureMatrix, float4(screenUV, 1.0));
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                uint4 h = _Histogram2.Load(uint(i.uv.x * _HistogramBucketCount));

                // float4 minLuminance = _HistogramMinMax.Load4(0);
                // float4 maxLuminance = _HistogramMinMax.Load4(4);
                uint minBuckets = _HistogramData[0].minBucketCount;
                uint maxBuckets = _HistogramData[0].maxBucketCount;

                // TODO: grid + colors

                float3 v = h.x / 1200;

                return float4(h.x, 0, 0, 1);

                return float4(i.uv.yyy < v.xxx, 1) * tex2D(_GUIClipTexture, i.clipUV).a;
            }
            ENDCG
        }
    }
}
