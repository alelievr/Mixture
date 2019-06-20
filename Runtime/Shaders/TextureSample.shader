Shader "Hidden/Mixture/TextureSample"
{	
    Properties
    {
		[MixtureTexture2D]_Texture("Texture", 2D) = "white" {}
		[MixtureTexture2D]_UV("UV", 2D) = "white" {}
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

			sampler2D _Texture;
			sampler2D _UV;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
				float2 uv = tex2Dlod(_UV, float4(i.uv,0,0)).rg;
                float4 col = tex2D(_Texture, uv);
                return col;
            }
            ENDCG
        }
    }
}
