Shader "Hidden/Mixture/Rasterize3D"
{	
	Properties
	{
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
            Cull Off
            ColorMask 0
            ZWrite Off
            ZClip Off

			CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.5
            #pragma enable_d3d11_debug_symbols

            #include "UnityCG.cginc"

            RWTexture3D<float4> _Output : register(u2);
            float2              _OutputSize;

            // struct appdata
            // {
            //     float4 vertex : POSITION;
            //     float2 uv : TEXCOORD0;
            // };

            // struct v2f
            // {
            //     float2 uv : TEXCOORD0;
            //     UNITY_FOG_COORDS(1)
            //     float4 vertex : SV_POSITION;
            // };

            // v2f vert (appdata v)
            // {
            //     v2f o;
            //     o.vertex = float4(v.vertex.xyz, 1);
            //     o.uv = v.uv;
            //     UNITY_TRANSFER_FOG(o,o.vertex);
            //     _Output[uint3(0, 0, 0)] = 1;
            //     return o;
            // }
            
            // fixed4 frag (v2f i) : SV_Target
            // {
            //     float3 pos = vertex.xyz;

            //     if (any(pos < 0))
            //         return 0;
            //     if (any(pos > 2))
            //         return 0;
                
            //     pos /= 2 * _OutputSize.x;
            //     _Output[uint3(pos)] = 1;
            //     return 0;
            // }

            float _Dir;

            struct VertexToFragment
            {
                float4 position : SV_POSITION;
                float3 normal : TEXCOORD0;
            };

            VertexToFragment vert(float4 vertex : POSITION, float3 normal : NORMAL)
            {
                VertexToFragment o;

                o.position = float4(mul(unity_ObjectToWorld, vertex).xyz, 1);
                o.normal = normal;

                return o;
            }

            fixed4 frag(VertexToFragment i) : COLOR
            {
                float3 pos = i.position.xyz;

                pos.z = pos.z * 0.5 + 0.5;
                pos.z *= _OutputSize.x;

                switch (_Dir)
                {
                    case 1:
                        pos = pos.yzx;
                        break;
                    case 2:
                        pos = pos.zyx;
                        break;
                }

                if (any(pos < 0) || any(pos > _OutputSize.x))
                    return 0;

                _Output[uint3(pos)] = 1;

                return 0;
            }
			ENDCG
		}
	}
}
