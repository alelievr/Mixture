Shader "Hidden/MixtureTexture3DPreview"
{
    Properties
    {
        _Texture3D ("Texture", 3D) = "" {}
        _Depth ("Depth", Float) = 0
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
            // SamplerState sampler_Point_Clamp_Texture3D;
            SamplerState sampler_Linear_Clamp_Texture3D;
            float4 _Texture3D_ST;
            float4 _Texture3D_TexelSize;
            float _Depth;

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
                // For debug we can use a point sampler: TODO make it an option in the UI
                // return _Texture3D.Sample(sampler_Point_Clamp_Texture3D, float3(i.uv, _Depth));
                return _Texture3D.Sample(sampler_Linear_Clamp_Texture3D, float3(i.uv, _Depth));
            }
            ENDCG
        }
    }
}
