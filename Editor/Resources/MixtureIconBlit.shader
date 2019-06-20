Shader "Hidden/MixtureIconBlit"
{
    Properties
    {
        _MixtureIcon ("Texture", 2D) = "" {}
        _Texture ("Texture", 2D) = "" {}
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

            // TODO
            // #pragma multi_compile

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

            UNITY_DECLARE_TEX2D(_MixtureIcon);
            float4 _MixtureIcon_ST;
            UNITY_DECLARE_TEX2D(_Texture);
            float4 _Texture_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MixtureIcon);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				float2 iconUV = i.uv * 2.5;
                // sample the texture
                fixed4 iconColor = 0;
				if (all(iconUV < 1))
					iconColor = UNITY_SAMPLE_TEX2D(_MixtureIcon, iconUV);
                fixed4 t = UNITY_SAMPLE_TEX2D(_Texture, i.uv);
                return iconColor + t;
            }
            ENDCG
        }
    }
}
