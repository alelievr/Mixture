Shader "Hidden/HistogramPreview"
{
    Properties
    {
    }
	SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 5.0

            #pragma shader_feature CRT_2D CRT_3D CRT_CUBE

            #include "Packages/com.alelievr.mixture/Runtime/Shaders/MixtureUtils.cginc"

            ByteAddressBuffer _Histogram;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                uint h = _Histogram.Load(uint(i.uv.x * 256));

                float v = float(h) / 100000;

                return i.uv.y < v ? 1 : 0;
            }
            ENDCG
        }
    }
}
