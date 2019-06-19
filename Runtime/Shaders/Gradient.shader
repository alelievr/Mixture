Shader "Hidden/Mixture/Gradient"
{	
    Properties
    {
		[Enum(Horizontal,0,Vertical,1,Radial,2,Circular,3)]_Mode("Gradient Type", Float) = 0
		[HDR]_Color1("Color 1", Color) = (0.0,0.0,0.0,0.0)
		[HDR]_Color2("Color 2", Color) = (1.0,1.0,1.0,1.0)
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

			float _Mode;
			float4 _Color1;
			float4 _Color2;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
				float2 uv = float2(i.uv.x, i.uv.y);
				float gradient = 0.0f;

				uint mode = (uint)_Mode;
				switch (mode)
				{
					case 0: gradient = uv.x; break;
					case 1: gradient = uv.y; break;
					case 2: uv -= 0.5; gradient = pow(saturate(1.0 - (dot(uv, uv) * 4.0)), 2.0); break;
					case 3: uv -= 0.5; gradient = saturate((atan2(uv.y, uv.x) / 6.283185307179586476924) + 0.5); break;
				}
				return lerp(_Color1,_Color2,gradient);
            }
            ENDCG
        }
    }
}
