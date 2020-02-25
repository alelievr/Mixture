Shader "Custom Mip Map"
{
    Properties
    {
    }
    SubShader
    {
    	Pass
    	{
    		Lighting Off
    		Blend One Zero

    		CGPROGRAM
    		#pragma vertex vert
    		#pragma fragment frag
    		#pragma target 3.0

    		#pragma shader_feature CRT_2D CRT_3D CRT_CUBE

    		#include "Packages/com.alelievr.mixture/Runtime/Shaders/MixtureUtils.cginc"

    		// Global variables required for mip chain generation
    		TEXTURE_SAMPLER_X(_InputTexture);
    		float _CurrentMipLevel;
    		float _CurrentSlice; // for 3D textures

    		struct SurfaceDescriptionInputs
    		{
    			// update input values
    			float4	uv0;

    			// ShaderGraph accessors:
    			float3 TimeParameters;
    		};

    		struct VertexToFragment
    		{
    			float4 positionCS : SV_POSITION;
    			float2 texcoord : TEXCOORD0;
    		};

    		SurfaceDescriptionInputs ConvertV2FToSurfaceInputs( VertexToFragment IN )
    		{
    			SurfaceDescriptionInputs o;

    			o.uv0 = float4(IN.texcoord, _CurrentSlice, 0);

    			// other space of view direction are not supported
    			o.TimeParameters = float3(_Time.y, _SinTime.x, _CosTime.y);

    			return o;
    		}

    		bool IsGammaSpace()
    		{
    		#ifdef UNITY_COLORSPACE_GAMMA
    			return true;
    		#else
    			return false;
    		#endif
    		}
    		float4 SRGBToLinear( float4 c ) { return c; }
    		float3 SRGBToLinear( float3 c ) { return c; }


    		///----------------------------------------------------------
    		/// Begin Generated Graph Code
    		///----------------------------------------------------------
        	    CBUFFER_START(UnityPerMaterial)
        CBUFFER_END

        void Unity_Add_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A + B;
        }

        struct SurfaceDescription
        {
            float4 Color;
        };

        SurfaceDescription PopulateSurfaceData(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            float4 _UV_89C5C998_Out_0 = IN.uv0;
            
    float4 _SampleCurrentMip_399C21DD_Color_1 = SAMPLE_LOD_X(_InputTexture, (_UV_89C5C998_Out_0.xyz), (_UV_89C5C998_Out_0.xyz), _CurrentMipLevel);
    float _SampleCurrentMip_399C21DD_MipLevel_2 = _CurrentMipLevel;
                
            float4 _Add_D89BADF7_Out_2;
            Unity_Add_float4(_SampleCurrentMip_399C21DD_Color_1, float4(0.2, 0.1, 0.05, 0), _Add_D89BADF7_Out_2);
            surface.Color = _Add_D89BADF7_Out_2;
            return surface;
        }


    		///----------------------------------------------------------
    		/// End Generated Graph Code
    		///----------------------------------------------------------

    		struct VertexData
    		{
    			uint vertexID : SV_VertexID;
    		};

    		VertexToFragment vert(VertexData IN)
    		{
    			VertexToFragment o;

    			// We only need UV to generate mipmaps
    			o.texcoord = GetFullScreenTriangleTexCoord(IN.vertexID);
    			o.positionCS = GetFullScreenTriangleVertexPosition(IN.vertexID);

    			return o;
    		}

    		float4 frag(VertexToFragment IN) : COLOR
    		{
    			SurfaceDescriptionInputs surfaceInput = ConvertV2FToSurfaceInputs(IN);

    			SurfaceDescription surf = PopulateSurfaceData(surfaceInput);

    			return surf.Color;
    		}
    		ENDCG
    	}
    }
    FallBack "Hidden/Shader Graph/FallbackError"
}
