Shader "Hidden/Mixture/Flip"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _FlipX ("Flip X", Float) = 0
        _FlipY ("Flip Y", Float) = 0
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

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

            float _FlipX;
            float _FlipY;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;

                o.uv.x = _FlipX > 0 ? 1 - o.uv.x : o.uv.x;
                o.uv.y = _FlipY > 0 ? 1 - o.uv.y : o.uv.y;
                return o;
            }

            sampler2D _MainTex;

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                return col;
            }
            ENDCG
        }
    }
}