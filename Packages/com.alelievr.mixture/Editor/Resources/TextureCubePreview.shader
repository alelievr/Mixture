Shader "Hidden/MixtureTextureCubePreview"
{
    Properties
    {
        _Cubemap ("Texture", Cube) = "" {}
		_Channels ("_Channels", Vector) = (1.0,1.0,1.0,1.0)
		_PreviewMip("_PreviewMip", Float) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Overlay" }
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
			Cull Back
			ZWrite Off
            ZTest Off
			ZTest LEqual

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

			#include "Packages/com.alelievr.mixture/Runtime/Shaders/MixtureFixed.cginc"

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

            UNITY_DECLARE_TEXCUBE(_Cubemap);
            float4 _Cubemap_ST;
            float4 _Cubemap_TexelSize;
			float4 _Channels;
			float _PreviewMip;
			float _SRGB;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _Cubemap);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture

                // TODO: factorize this !
                float2 checkerboardUVs = ceil(fmod(i.uv * _Cubemap_TexelSize.zw / 64.0, 1.0)-0.5);
				float3 checkerboard = lerp(0.3,0.4, checkerboardUVs.x != checkerboardUVs.y ? 1 : 0);
                float4 color = SAMPLE_TEXTURECUBE_LOD(_Cubemap, s_linear_clamp_sampler, normalize(LatlongToDirectionCoordinate(i.uv)), 0) * _Channels;

                if (_Channels.a == 0.0) 
					color.a = 1.0;

				else if (_Channels.r == 0.0 && _Channels.g == 0.0 && _Channels.b == 0.0 && _Channels.a == 1.0)
				{
					color.rgb = color.a;
					color.a = 1.0;
				}
				color.xyz = pow(color.xyz, 1.0 / 2.2);

				return float4(lerp(checkerboard, color.xyz, color.a),1);
            }
            ENDCG
        }
    }
}
