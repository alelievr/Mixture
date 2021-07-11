Shader "Hidden/MixtureInspectorPreview"
{
    Properties
    {
        _MainTex0_2D ("_MainTex 0", 2D) = "" {}
        _MainTex1_2D ("_MainTex 1", 2D) = "" {}
        _MainTex0_3D ("_MainTex 0", 3D) = "" {}
        _MainTex1_3D ("_MainTex 1", 3D) = "" {}
        _MainTex0_Cube ("_MainTex 0", Cube) = "" {}
        _MainTex1_Cube ("_MainTex 1", Cube) = "" {}
		_Size("_Size", Vector) = (512.0,512.0,1.0,1.0)
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
			Cull Off
			ZWrite Off
			ZTest LEqual


            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature CRT_2D CRT_3D CRT_CUBE
            // #pragma enable_d3d11_debug_symbols

			#include "Packages/com.alelievr.mixture/Editor/Resources/MixturePreview.hlsl"
            #include "Packages/com.alelievr.mixture/Runtime/Shaders/MixtureSRGB.hlsl"

            float4 _TextureSize;
            float _ComparisonSlider;
            float _ComparisonSlider3D;
            float4 _MouseUV;
            float _Zoom;
            float4 _Pan;
            float _YRatio;
            float _Exp;
            float _FilterMode;
            float _CompareMode;
            float _ComparisonEnabled;
            float _IsSRGB0;
            float _IsSRGB1;
            float _PreserveAspect;
            float _CameraZoom;
            float4x4 _CameraMatrix;
            float _Density;
            float _SDFOffset;
            float _Texture3DMode;
            float _ShowCubeBackface;
            float _InvertSurface;
            float _VolumetricDensityChannel;

            #define MERGE_NAME(x, y) x##y

            sampler s_trilinear_repeat_sampler;

#if CRT_2D
			Texture2D _MainTex0_2D;
			Texture2D _MainTex1_2D;
            #define TEXTURE_TYPE Texture2D

            #define SAMPLE_LEVEL(tex, samp, uv, mip) MERGE_NAME(tex,_2D).SampleLevel(samp, uv.xy, mip)
#elif CRT_3D
			Texture3D _MainTex0_3D;
			Texture3D _MainTex1_3D;
            #define TEXTURE_TYPE Texture3D

            // #define SAMPLE_LEVEL(tex, samp, uv, mip) MERGE_NAME(tex,_3D).SampleLevel(samp, uv + float3(0, 0, 0.5), mip)
            #define SAMPLE_LEVEL(tex, samp, uv, mip) SampleTexture3D(MERGE_NAME(tex,_3D), samp, uv, mip)

            float4 SampleTexture3D(Texture3D volume, SamplerState samp, float3 uv, float mip)
            {
                // UV can be seen as 3D object space pos
                float3 objectCenter = float3(0, 0, 0);

                float3 target = float3(0., 0., 0.);
                float3 ro = mul(_CameraMatrix, float4(0, 0, -_CameraZoom, 0)).xyz;
                float3 rd = normalize(mul(_CameraMatrix, float4(uv.x * 2 - 1, uv.y * 2 - 1, 4, 0))).xyz;

                float2 boxIntersection = RayBoxIntersection(ro, rd, 1 - 0.000001);

                if (boxIntersection.y < 0)
                    return 0;
                else
                {
                    boxIntersection.x = max(boxIntersection.x, 0.0);

                    switch (_Texture3DMode)
                    {
                        default:
                        case 0: // Volume
                            return RayMarchVolume(ro, rd, volume, samp, mip, boxIntersection.x, boxIntersection.y, _Density, _VolumetricDensityChannel);
                        case 1: // SDF
                            return RayMarchSDF(ro, rd, volume, samp, mip, boxIntersection.x, boxIntersection.y, _SDFOffset, true, _InvertSurface);
                        case 2: // SDF
                            return RayMarchSDF(ro, rd, volume, samp, mip, boxIntersection.x, boxIntersection.y, _SDFOffset, false, _InvertSurface);
                    }
                    // TODO: send max distance to raymarcher
                }
            }
#elif CRT_CUBE
			TextureCube _MainTex0_Cube;
			TextureCube _MainTex1_Cube;
            #define TEXTURE_TYPE TextureCube

            #define SAMPLE_LEVEL(tex, samp, uv, mip) SampleCubemap(MERGE_NAME(tex,_Cube), samp, uv, mip)

            float4 SampleCubemap(TextureCube cube, SamplerState samp, float3 uv, float mip)
            {
                // UV can be seen as 3D object space pos
                float3 objectCenter = float3(0, 0, 0);

                if (_ShowCubeBackface)
                    uv = float3(1 - uv.x, uv.y, 0);
                
                float3 target = float3(0., 0., 0.);
                float3 ro = mul(_CameraMatrix, float4(0, 0, -_CameraZoom, 0)).xyz;
                float3 rd = normalize(mul(_CameraMatrix, float4(uv.x * 2 - 1, uv.y * 2 - 1, 4, 0))).xyz;

                float2 sphereIntersection = RaySphereIntersection(ro, rd, 0, 1 - 0.000001);

                if (all(sphereIntersection == -1))
                    return 0;
                else
                {
                    float3 hit;

                    if (_ShowCubeBackface)
                        hit = ro + rd * sphereIntersection.y;
                    else
                        hit = ro + rd * sphereIntersection.x;

                    return cube.SampleLevel(samp, hit, mip);
                }
            }
#endif

            float4 ApplyComparison(float2 uv, float4 c0, float4 c1)
            {
                if (_ComparisonEnabled.x == 0)
                {
                    return c0;
                }
                else
                {
                    float comparisonSlider = _ComparisonSlider;
#if defined(CRT_3D) || defined(CRT_CUBE)
                    comparisonSlider = _ComparisonSlider3D;
#endif

                    switch (_CompareMode)
                    {
                        default:
                        case 0: // Side By Side
                            return frac(uv.x) < comparisonSlider ? c0 : c1;
                        case 1: // Onion skin
                            return lerp(c0, c1, comparisonSlider);
                        case 2: // Difference
                            return abs(c0 - c1);
                        case 3: // Swap
                            return _MouseUV.x > 0.5 ? c0 : c1;
                    }
                }
            }

            float4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;
                float4 color0, color1;

// 3D and cube texture have the camera zoom instead of UV zoom
#ifdef CRT_2D 
                uv += float2(-_Pan.x, _Pan.y - 1);
                uv *= rcp(_Zoom.xx);
#endif

                if (_PreserveAspect > 0)
                {
                    if (_YRatio > 1)
                        uv.y *= _YRatio;
                    else
                    {
                        uv.x -= (1 - _YRatio)/2;
                        uv.x *= rcp(_YRatio);
                    }
                }

                switch (_FilterMode)
                {
                    default:
                    case 0: // Point
                        color0 = SAMPLE_LEVEL(_MainTex0, s_point_repeat_sampler, float3(uv, 0), floor(_PreviewMip)) * _Channels;
                        color1 = SAMPLE_LEVEL(_MainTex1, s_point_repeat_sampler, float3(uv, 0), floor(_PreviewMip)) * _Channels;
                        break;
                    case 1: // Bilinear
                        color0 = SAMPLE_LEVEL(_MainTex0, s_linear_repeat_sampler, float3(uv, 0), floor(_PreviewMip)) * _Channels;
                        color1 = SAMPLE_LEVEL(_MainTex1, s_linear_repeat_sampler, float3(uv, 0), floor(_PreviewMip)) * _Channels;
                        break;
                    case 2: // Trilinear
                        color0 = SAMPLE_LEVEL(_MainTex0, s_trilinear_repeat_sampler, float3(uv, 0), floor(_PreviewMip)) * _Channels;
                        color1 = SAMPLE_LEVEL(_MainTex1, s_trilinear_repeat_sampler, float3(uv, 0), floor(_PreviewMip)) * _Channels;
                        break;
                }

                // Apply gamma if needed
                if (_IsSRGB0)
                    color0.xyz = LinearToSRGB(color0.xyz);
                if (_IsSRGB1)
                    color1.xyz = LinearToSRGB(color1.xyz);

                // TODO: blend the two colors with comparison mode
                float4 color = ApplyComparison(uv, color0, color1);

                // Apply exposure:
                color.rgb = color.rgb * exp2(_Exp);

                return MakePreviewColor(i, _TextureSize.zw, color);
            }
            ENDHLSL
        }
    }
}
