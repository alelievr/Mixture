Shader "Hidden/MixtureTexture2DPreview"
{
    Properties
    {
        _MainTex ("_MainTex", 2D) = "" {}
		_Size("_Size", Vector) = (512.0,512.0,1.0,1.0)
		_Channels ("_Channels", Vector) = (1.0,1.0,1.0,1.0)
		_Mip("_Mip", Float) = 0.0
		_SRGB("_SRGB", Float) = 0.0
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

			sampler2D _MainTex;
			float4 _MainTex_ST;
            float4 _Size;
			float4 _Channels;
			float _Mip;
			float _SRGB;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
				float2 uvs = ceil(fmod(i.uv * _Size.xy / 64.0, 1.0)-0.5);
				float3 checkerboard = lerp(0.3,0.4,uvs.x != uvs.y ? 1 : 0);
				float4 color = tex2Dlod(_MainTex, float4(i.uv, 0.0, _Mip)) * _Channels;
				
				if (_Channels.a == 0.0) 
					color.a = 1.0;
				if (_SRGB == 1.0)
					color.xyz = pow(color.xyz, 2.2);
				return float4(lerp(checkerboard, color.xyz, color.a),1);
            }
            ENDCG
        }
    }
}
