Shader "Hidden/MixtureTexture3DPreview"
{
    Properties
    {
        _Texture3D ("Texture", 3D) = "" {}
        _Depth ("Depth", Float) = 0
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

            UNITY_DECLARE_TEX3D(_Texture3D);
            // SamplerState sampler_Point_Clamp_Texture3D;
            SamplerState sampler_Linear_Clamp_Texture3D;
            float4 _Texture3D_ST;
            float4 _Texture3D_TexelSize;
            float _Depth;
			float4 _Channels;
			float _PreviewMip;
			float _SRGB;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _Texture3D);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // For debug we can use a point sampler: TODO make it an option in the UI
                // return _Texture3D.Sample(sampler_Point_Clamp_Texture3D, float3(i.uv, _Depth));

                // TODO: factorize this !
				float2 checkerboardUVs = ceil(fmod(i.uv * _Texture3D_TexelSize.xy / 64.0, 1.0)-0.5);
				float3 checkerboard = lerp(0.3,0.4, checkerboardUVs.x != checkerboardUVs.y ? 1 : 0);
                float4 color = _Texture3D.SampleLevel(sampler_Linear_Clamp_Texture3D, float3(i.uv, _Depth), _PreviewMip) * _Channels;

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
