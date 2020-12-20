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
            // #pragma enable_d3d11_debug_symbols

            #include "Packages/com.alelievr.mixture/Runtime/Shaders/MixtureUtils.cginc"
            #include "Packages/com.alelievr.mixture/Editor/Resources/HistogramData.hlsl"

            StructuredBuffer<HistogramBucket>   _HistogramReadOnly;
            StructuredBuffer<HistogramData>     _HistogramDataReadOnly;
            float _Mode;

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

            float4 frag (v2f i) : SV_Target
            {
                HistogramBucket b = _HistogramReadOnly[uint(i.uv.x * _HistogramBucketCount)];

                // float4 minLuminance = _HistogramMinMax.Load4(0);
                // float4 maxLuminance = _HistogramMinMax.Load4(4);
                uint minBuckets = _HistogramDataReadOnly[0].minBucketCount;
                uint maxBuckets = _HistogramDataReadOnly[0].maxBucketCount;

                // TODO: min bucket range when it works
                float4 data = float4(b.luminance, b.r, b.g, b.b) / maxBuckets;

                float3 histogram = 0;
                switch (_Mode)
                {
                    case 0: // Luminance
                        histogram = data.xxx;
                        break;
                    case 1: // Color
                        histogram = data.yzw;
                        break;
                }

                histogram = i.uv.yyy < histogram;

                // if (all(histogram == 1))
                //     histogram = 0.8;

                // "Beautify" the colorss:
                histogram *= 0.8;

                return float4(histogram, 1) * tex2D(_GUIClipTexture, i.clipUV).a;
            }
            ENDCG
        }
    }
}
