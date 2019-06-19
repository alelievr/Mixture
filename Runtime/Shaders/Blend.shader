Shader "Hidden/Mixture/Blend"
{	
    Properties
    {
		[MixtureTexture2D]_Source("Source", 2D) = "white" {}
		[MixtureTexture2D]_Target("Target", 2D) = "white" {}
		[MixtureTexture2D]_Mask("Mask", 2D) = "white" {}
		[Enum(Blend,0,Additive,1,Multiplicative,2)]_Mode("Blend Mode", Float) = 0
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
			sampler2D _Target;
			sampler2D _Mask;
			float _Mode;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
				float4 uv = float4(i.uv.x, i.uv.y, 0, 0);

				float4	source	= tex2Dlod(_Source, uv);
				float4	target	= tex2Dlod(_Target, uv);
				float	mask	= tex2Dlod(_Mask, uv).a;

				uint mode = (uint)_Mode;
				switch (mode)
				{
				case 0: return lerp(source, target, mask);
				case 1: return lerp(source, source + target, mask);
				case 2: return lerp(source, source * target, mask);
				}
				// Should not happen but hey...
				return float4(1.0, 0.0, 1.0, 1.0);
            }
            ENDCG
        }
    }
}
