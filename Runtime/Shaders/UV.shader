Shader "Hidden/Mixture/UV"
{	
    Properties
    {
		[MixtureVector2]_Scale("UV Scale", Vector) = (1.0,1.0,0.0,0.0)
		[MixtureVector2]_Bias("UV Bias", Vector) = (0.0,0.0,0.0,0.0)
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

			float4 _Scale;
			float4 _Bias;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = (v.uv * _Scale.xy) + _Bias.xy;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                return float4(i.uv.x,i.uv.y,0,1);
            }
            ENDCG
        }
    }
}
