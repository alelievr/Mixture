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
            #pragma enable_d3d11_debug_symbols

            #include "UnityCG.cginc"

            RWTexture3D<float>  _Output : register(u2);
            float2              _OutputSize;
            float               _Dir;
            float4x4            _CameraMatrix;

            ByteAddressBuffer _MeshVertices;
            ByteAddressBuffer _MeshIndices;

            struct VertexToFragment
            {
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD0;
            };

            VertexToFragment vert(uint vertexId : SV_VERTEXID)
            {
                VertexToFragment o;

                uint triangleId = vertexId - (vertexId % 3);
                uint t0 = _MeshIndices.Load(triangleId + 0);
                uint t1 = _MeshIndices.Load(triangleId + 1);
                uint t2 = _MeshIndices.Load(triangleId + 2);

                // Fetch triangle vertex data:
                float3 v0 = _MeshVertices.Load3(t0);
                float3 v1 = _MeshVertices.Load3(t1);
                float3 v2 = _MeshVertices.Load3(t2);

                // Calculate centroid of the triangle
                float3 c = (v0 + v1 + v2) / 3;

                // TODO
                // rotate the triangle towards -Z
                float3 n = cross(v0, v1);

                // o.vertex = mul(_CameraMatrix, float4(vertex.xyz, 1.0));
                // o.worldPos = mul (unity_ObjectToWorld, vertex);

                float3 v = _MeshVertices.Load3(vertexId);
                o.vertex = mul(_CameraMatrix, float4(v.xyz, 1.0));
                o.worldPos = mul (unity_ObjectToWorld, v);

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
