Shader "Hidden/MixtureTextureCubePreview"
{
    Properties
    {
        _Cubemap ("Texture", Cube) = "" {}
        _Slice ("Slice", Float) = 0
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

            #include "UnityCG.cginc"
            #include "MixtureUtils.cginc"

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

            UNITY_DECLARE_TEXCUBE(_Cubemap);
            float4 _Cubemap_ST;
            float _Slice;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _Cubemap);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                return UNITY_SAMPLE_TEXCUBE(_Cubemap, LatlongToDirectionCoordinate(i.uv));
            }
            ENDCG
        }
    }
}
