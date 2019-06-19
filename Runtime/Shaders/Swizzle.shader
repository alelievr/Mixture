Shader "Hidden/Mixture/Swizzle"
{	
    Properties
    {
		[MixtureTexture2D]_Source("Input", 2D) = "white" {}
		[MixtureSwizzle]_RMode("Output Red", Float) = 0
		[MixtureSwizzle]_GMode("Output Green", Float) = 1
		[MixtureSwizzle]_BMode("Output Blue", Float) = 2
		[MixtureSwizzle]_AMode("Output Alpha", Float) = 3
		[HDR]_Custom("Custom", Color) = (1.0,1.0,1.0,1.0)
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

			sampler2D _Source;
			float _RMode;
			float _GMode;
			float _BMode;
			float _AMode;
			float4 _Custom;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
                return o;
            }

			float Swizzle(float4 sourceValue, uint mode, float custom)
			{
				switch (mode)
				{
				case 0: return sourceValue.x;
				case 1: return sourceValue.y;
				case 2: return sourceValue.z;
				case 3: return sourceValue.w;
				case 4: return 0.0f;
				case 5: return 0.5f;
				case 6: return 1.0f;
				case 7: return custom;
				}
				return 0;
			}

            float4 frag (v2f i) : SV_Target
            {
				float4 uv = float4(i.uv.x, i.uv.y, 0, 0);

				float4	source	= tex2Dlod(_Source, uv);
				float r = Swizzle(source, _RMode, _Custom.r);
				float g = Swizzle(source, _GMode, _Custom.g);
				float b = Swizzle(source, _BMode, _Custom.b);
				float a = Swizzle(source, _AMode, _Custom.a);
				return float4(r,g,b,a);
            }
            ENDCG
        }
    }
}
