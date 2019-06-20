Shader "Hidden/Mixture/ScaleBias"
{	
    Properties
    {
		[MixtureTexture2D]_Texture("Texture", 2D) = "white" {}
		[Enum(ScaleBias,0,BiasScale,1,Scale,2,Bias,3)]_Mode("Mode", Float) = 0
		_Scale("Scale", Vector) = (1.0,1.0,0.0,0.0)
		_Bias("Bias", Vector) = (0.0,0.0,0.0,0.0)
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
			float _Mode;
			float4 _Scale;
			float4 _Bias;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float4 col = tex2D(_Texture, i.uv);
				uint mode = (uint)_Mode;
				switch (mode)
				{
				case 0: col = (col * _Scale) + _Bias; break;
				case 1: col = (col + _Bias) + _Scale; break;
				case 2: col = col * _Scale; break;
				case 3: col = col + _Bias; break;
				}
                return col;
            }
            ENDCG
        }
    }
}
