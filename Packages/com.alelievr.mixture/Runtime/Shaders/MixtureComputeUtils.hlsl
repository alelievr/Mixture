#ifndef MIXTURE_COMPUTE_UTILS
#define MIXTURE_COMPUTE_UTILS

float _TextureDimension;

#include "Packages/com.alelievr.mixture/Runtime/Shaders/CustomTexture.hlsl"
#include "Packages/com.alelievr.mixture/Runtime/Shaders/MixtureUtils.cginc"

#ifndef MERGE_NAME
# define MERGE_NAME(p1,p2) p1##p2
#endif

#ifdef TEXTURE_X
#undef TEXTURE_X
#endif
#ifdef SAMPLE_X
#undef SAMPLE_X
#endif

#define TEXTURE_X(name) Texture2D MERGE_NAME(name,_2D); Texture3D MERGE_NAME(name,_3D); TextureCube MERGE_NAME(name,_Cube)
#define RWTEXTURE_X(type, name) RWTexture2D<type> MERGE_NAME(name,_2D); RWTexture3D<type> MERGE_NAME(name,_3D)

#define SAMPLE_X(name, samp, uv, direction) SampleX(MERGE_NAME(name,_2D), MERGE_NAME(name,_3D), MERGE_NAME(name,_Cube), samp, uv, direction)

float4 SampleX(Texture2D tex2D, Texture3D tex3D, TextureCube texCube, sampler samp, float3 uv, float3 direction)
{
    // See TextureDimension enum values
    if (_TextureDimension == 2) // 2D
        return SAMPLE_TEXTURE2D_LOD(tex2D, samp, uv.xy, 0);
    else if (_TextureDimension == 3) // 3D
        return SAMPLE_TEXTURE3D_LOD(tex3D, samp, uv.xyz, 0);
    else if (_TextureDimension == 4) // Cube
        return SAMPLE_TEXTURECUBE_LOD(texCube, samp, direction.xyz, 0);
    else // not supported
        return 0;
}

#define WRITE_X(name, id, value) WriteX(MERGE_NAME(name,_2D), MERGE_NAME(name,_3D), id, value)


void WriteX(RWTexture2D<float4> tex2D, RWTexture3D<float4> tex3D, uint3 id, float4 value)
{
    if (_TextureDimension == 2) // 2D
        tex2D[id.xy] = value;
    else if (_TextureDimension == 3) // 3D
        tex3D[id.xyz] = value;
}

float3 GetDefaultUVsComputeShader(uint3 id, float3 rcpTextureSize)
{
#ifdef CRT_CUBE
    return ComputeCubemapDirectionFromUV(id.xy * rcpTextureSize.xy, id.z);
#elif CRT_2D
    return float3(id.xy * rcpTextureSize.xy, 0.5);
#else
    return id * rcpTextureSize;
#endif
}

#endif