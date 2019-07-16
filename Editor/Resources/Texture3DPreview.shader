Shader "Hidden/MixtureTexture3DPreview"
{
    Properties
    {
        _Texture3D ("Texture", 3D) = "" {}
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

            UNITY_DECLARE_TEX3D(_Texture3D);
            float4 _Texture3D_ST;
            float _Slice;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _Texture3D);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                return UNITY_SAMPLE_TEX3D(_Texture3D, float3(i.uv, _Slice));
            }
            ENDCG
        }
    }
}
