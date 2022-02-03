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
            Name "Standard Voxelization"
            Cull Off
            ColorMask 0
            ZWrite Off
            ZClip Off

			HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.5

            #include "UnityCG.cginc"

            RWTexture3D<float>  _Output : register(u2);
            float2              _OutputSize;
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

		Pass
		{
            Name "Single pass voxelization"
            Cull Off
            ColorMask 0
            ZWrite Off
            ZClip Off

			HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.5
            #pragma enable_d3d11_debug_symbols

            #include "UnityCG.cginc"

            RWTexture3D<float>  _Output : register(u2);
            float2              _OutputSize;
            float4x4            _CameraMatrix;

            int _VertexStride;
            int _VertexPositionOffset;

            struct OutputVertexData
            {
                float3 vertexPosition;
                float3 originalPosition;
            };

            // TODO: half3 position + original position output for vertex shader
            StructuredBuffer<OutputVertexData>  _OutputVertexPositions;

            struct VertexToFragment
            {
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD0;
            };

            VertexToFragment vert(uint vertexId : SV_VERTEXID)
            {
                VertexToFragment o;

                // uint triangleId = vertexId - (vertexId % 3);
                // uint t0 = _MeshIndices.Load(triangleId + 0);
                // uint t1 = _MeshIndices.Load(triangleId + 1);
                // uint t2 = _MeshIndices.Load(triangleId + 2);

                // // Fetch triangle vertex data:
                // float3 v0 = _MeshVertices.Load3(t0);
                // float3 v1 = _MeshVertices.Load3(t1);
                // float3 v2 = _MeshVertices.Load3(t2);

                // // Calculate centroid of the triangle
                // float3 c = (v0 + v1 + v2) / 3;

                // // TODO
                // // rotate the triangle towards -Z
                // float3 n = cross(v0, v1);

                // o.vertex = mul(_CameraMatrix, float4(vertex.xyz, 1.0));
                // o.worldPos = mul (unity_ObjectToWorld, vertex);

                float2 texelSize = rcp(_OutputSize);

                OutputVertexData v = _OutputVertexPositions.Load(vertexId);

                // Apply manual conservative
                // v.vertexPosition += normalize(v.vertexPosition) * texelSize.x;

                o.vertex = mul(_CameraMatrix, float4(v.vertexPosition, 1.0));
                o.worldPos = mul(unity_ObjectToWorld, v.originalPosition);

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

		Pass
		{
            Name "Hardware Raster Voxelization"
            Cull Off
            ZWrite Off
            ZClip Off
            ZTest Always

			HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.5
            #pragma enable_d3d11_debug_symbols

            #include "UnityCG.cginc"

            float2              _OutputSize;
            float4x4            _CameraMatrix;
            float4x4            _InvCameraMatrix;
            float3              _MinBounds;
            float3              _MaxBounds;
            float4x4            _LocalToWorld;

            struct VertexToFragment
            {
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                uint outputIndex : SV_RenderTargetArrayIndex;
            };

            float4 GetQuadVertexPosition(uint vertexID, float z = UNITY_NEAR_CLIP_VALUE)
            {
                uint topBit = vertexID >> 1;
                uint botBit = (vertexID & 1);
                float x = topBit;
                float y = 1 - (topBit + botBit) & 1; // produces 1 for indices 0,3 and 0 for 1,2
                return float4(x, y, z, 1.0);
            }

            VertexToFragment vert(float4 vertex : POSITION, uint vertexID : SV_VERTEXID, uint instanceId : SV_INSTANCEID)
            {
                VertexToFragment o;

                // o.vertex = mul(_CameraMatrix, float4(vertex.xyz, 1.0));
                // o.worldPos = mul (unity_ObjectToWorld, vertex);

                // Note: View space bounds are the same than actual bounds

                // Compute min depth slice index to write:
                uint minSliceDepth = (_MinBounds.z * 0.5 + 0.5) * _OutputSize.x;

                o.vertex = float4(-1, -1, instanceId / _OutputSize.x, 1);
                switch (vertexID % 6)
                {
                    case 1:
                        o.vertex.xy = float2(1, 1);
                        break;
                    case 2:
                        o.vertex.xy = float2(1, -1);
                        break;
                    case 3:
                        o.vertex.xy = float2(-1, -1);
                        break;
                    case 4:
                        o.vertex.xy = float2(-1, 1);
                        break;
                    case 5:
                        o.vertex.xy = float2(1, 1);
                        break;
                }

                // scale fullscreen quad based on view space bounds size:
                float3 boundsSize = _MaxBounds - _MinBounds;
                o.vertex.xyz *= boundsSize.xyz;
                o.vertex.xyz += (_MinBounds + boundsSize / 2.0);

                o.outputIndex = minSliceDepth + instanceId; // depth distribution is linear

                return o;
            }

            fixed4 frag(VertexToFragment i) : COLOR
            {
                // Test if point is outside bounds from transform:
                float3 localPosition = mul(_LocalToWorld, i.vertex.xyz);

                if (all(localPosition > _MinBounds))
                {
                    return 1;

                }
                else
                    return 0;
            }
			ENDHLSL
		}
	}
}
