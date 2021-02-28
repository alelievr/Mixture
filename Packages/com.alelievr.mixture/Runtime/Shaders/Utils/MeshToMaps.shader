Shader "Hidden/Mixture/MeshToMaps"
{	
	Properties
	{
		[Toggle]_Conservative("Concervative", Float) = 0
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
            Cull Off
            ZWrite Off
            ZClip Off
			Blend Off
			Conservative [_Conservative] 

			HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.5

			float _Mode;

			struct VertexInput
			{
				float4 vertex : POSITION;
                float2 uv0 : TEXCOORD0;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float4 vertexColor : COLOR;
			};

            struct VertexToFragment
            {
                float4 vertex : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 position : TEXCOORD1;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float4 vertexColor : COLOR;
            };

            VertexToFragment vert(VertexInput input)
            {
                VertexToFragment o;

                o.vertex = float4(input.uv0.xy * 2 - 1, 0.0, 1.0);
				o.uv0 = input.uv0;
				o.position = input.vertex;
				o.normal = input.normal;
				o.tangent = input.tangent;
				o.vertexColor = input.vertexColor;

                return o;
            }

            float4 frag(VertexToFragment i, uint primitiveId : SV_PrimitiveID) : COLOR
            {
				switch (_Mode)
				{
					default:
					case 0: // YUV
						return float4(i.uv0.xy, 0, 1);
					case 1: // Position
						return float4(i.position.xyz, 1);
					case 2: // Normal
						return float4(i.normal * 0.5 + 0.5, 1);
					case 3: // Tangent 
						return float4(i.tangent.xyz * 0.5 + 0.5, 1);
					case 4: // BiTangent 
						float3 biTangent = cross(i.normal, i.tangent.xyz) * i.tangent.w;
						return float4(biTangent * 0.5 + 0.5, 1);
					case 5: // IsFrontFace 
						float r = frac(primitiveId / 256.0f);
						float g = frac(primitiveId / 256.0f / 256.0f);
						float b = frac(primitiveId / 256.0f / 256.0f / 256.0f);
						return float4(r, g, b, 1);
					case 6: // VertexColor 
						return i.vertexColor;
				}
            }
			ENDHLSL
		}
	}
}
