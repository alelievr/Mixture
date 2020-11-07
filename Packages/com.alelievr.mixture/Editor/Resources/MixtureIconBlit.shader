Shader "Hidden/MixtureIconBlit"
{
    Properties
    {
        _MixtureIcon ("Texture", 2D) = "" {}
        _Texture2D ("Texture2D", 2D) = "" {}
        _Texture2DArray ("Texture2DArray", 2DArray) = "" {}
        _Texture3D ("Texture3D", 3D) = "" {}
        _Cubemap("Cubemap", Cube) = "" {}
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

            #pragma shader_feature CRT_2D CRT_2D_ARRAY CRT_3D CRT_CUBE

            #include "Packages/com.alelievr.mixture/Runtime/Shaders/MixtureUtils.cginc"

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
            float4 _Texture3D_TexelSize;
            UNITY_DECLARE_TEXCUBE(_Cubemap);
            float4 _Cubemap_ST;

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
#if CRT_2D
                fixed4 t = UNITY_SAMPLE_TEX2D(_Texture2D, i.uv);
#elif CRT_2D_ARRAY
                // For texture arrays, we take the first slice
                fixed4 t = UNITY_SAMPLE_TEX2DARRAY(_Texture2DArray, float3(i.uv, 0));
#elif CRT_3D
                // For texture 3D, we take the first slice
                fixed4 t = UNITY_SAMPLE_TEX3D(_Texture3D, float3(i.uv, _Texture3D_TexelSize.x / 2.0));
                // humm, i need a better way to visualize a Texture3D
                t.a = 1;
#elif CRT_CUBE
                fixed4 t = UNITY_SAMPLE_TEXCUBE(_Cubemap, LatlongToDirectionCoordinate(i.uv));
#endif

                return lerp(t, iconColor, iconColor.a);
            }
            ENDCG
        }
    }
}
