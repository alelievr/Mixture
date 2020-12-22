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
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma shader_feature CRT_2D CRT_2D_ARRAY CRT_3D CRT_CUBE

            #include "Packages/com.alelievr.mixture/Runtime/Shaders/MixtureUtils.hlsl"

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

            Texture2D _MixtureIcon;
            float4 _MixtureIcon_ST;
            Texture2D _Texture2D;
            sampler sampler_Texture2D;
            float4 _Texture2D_ST;
            Texture2DArray _Texture2DArray;
            float4 _Texture2DArray_ST;
            Texture3D _Texture3D;
            float4 _Texture3D_ST;
            float4 _Texture3D_TexelSize;
            TextureCube _Cubemap;
            float4 _Cubemap_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex.xyz);
                o.uv = TRANSFORM_TEX(v.uv, _MixtureIcon);
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
				float2 iconUV = i.uv * 2.5;
                // sample the texture
                float4 iconColor = 0;
				if (all(iconUV < 1))
					iconColor = _MixtureIcon.SampleLevel(s_linear_clamp_sampler, iconUV, 0);
#if CRT_2D
                float4 t = _Texture2D.SampleLevel(sampler_Texture2D, i.uv, 0);
#elif CRT_2D_ARRAY
                // For texture arrays, we take the first slice
                float4 t = UNITY_SAMPLE_TEX2DARRAY(_Texture2DArray, float3(i.uv, 0));
#elif CRT_3D
                // For texture 3D, we take the first slice
                float4 t = UNITY_SAMPLE_TEX3D(_Texture3D, float3(i.uv, _Texture3D_TexelSize.x / 2.0));
                // humm, i need a better way to visualize a Texture3D
                t.a = 1;
#elif CRT_CUBE
                float4 t = UNITY_SAMPLE_TEXCUBE(_Cubemap, LatlongToDirectionCoordinate(i.uv));
#endif

                return lerp(t, iconColor, iconColor.a);
            }
            ENDHLSL
        }
    }
}
