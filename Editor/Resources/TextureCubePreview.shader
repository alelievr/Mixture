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

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                return UNITY_SAMPLE_TEXCUBE(_Cubemap, LatlongToDirectionCoordinate(i.uv));
            }
            ENDCG
        }
    }
}
