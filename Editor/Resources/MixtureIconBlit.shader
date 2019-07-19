Shader "Hidden/MixtureIconBlit"
{
    Properties
    {
        _MixtureIcon ("Texture", 2D) = "" {}
        _Texture2D ("Texture2D", 2D) = "" {}
        _Texture2DArray ("Texture2DArray", 2DArray) = "" {}
        _Texture3D ("Texture3D", 3D) = "" {}
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

            // TOOD: multi compile for cubemap
            #pragma multi_compile TEXTURE2D TEXTURE2D_ARRAY TEXTURE3D

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
            UNITY_DECLARE_TEX2D(_Texture2D);
            float4 _Texture2D_ST;
            UNITY_DECLARE_TEX2DARRAY(_Texture2DArray);
            float4 _Texture2DArray_ST;
            UNITY_DECLARE_TEX3D(_Texture3D);
            float4 _Texture3D_ST;

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
#if TEXTURE2D
                fixed4 t = UNITY_SAMPLE_TEX2D(_Texture2D, i.uv);
#elif TEXTURE2D_ARRAY
                // For texture arrays, we take the first slice
                fixed4 t = UNITY_SAMPLE_TEX2DARRAY(_Texture2DArray, float3(i.uv, 0));
#elif TEXTURE3D
                // For texture 3D, we take the first slice
                fixed4 t = UNITY_SAMPLE_TEX3D(_Texture3D, float3(i.uv, 0));
#endif
                return lerp(t, iconColor, iconColor.a);
            }
            ENDCG
        }
    }
}
