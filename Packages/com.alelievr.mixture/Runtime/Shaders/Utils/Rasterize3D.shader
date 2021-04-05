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
            Conservative [_ConservativeRaster]

			HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.5

            #include "UnityCG.cginc"

            RWTexture3D<float>  _Output : register(u2);
            float2              _OutputSize;
            float               _Dir;
            float4x4            _CameraMatrix;

            struct VertexToFragment
            {
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD0;
            };

            VertexToFragment vert(float4 vertex : POSITION)
            {
                VertexToFragment o;

                o.vertex = mul(_CameraMatrix, float4(vertex.xyz, 1.0));
                o.worldPos = mul (unity_ObjectToWorld, vertex);

                return o;
            }

            fixed4 frag(VertexToFragment i) : COLOR
            {
                float3 pos = (i.worldPos * 0.5 + 0.5) * _OutputSize.x;

                if (any(pos < 0) || any(pos > _OutputSize.x))
                    return 0;

                _Output[uint3(pos)] = 1;

                return 0;
            }
			ENDHLSL
		}
	}
}
