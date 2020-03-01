// STEP_LENGTH needs to be defined to call this function
float4 mixture (v2f_customrendertexture crt) : SV_Target
{
    // The SAMPLE_X macro handles sampling for 2D, 3D and cube textures

    // TODO: non-pot handling
    float3 offset = (1 << STEP_LENGTH) / _CustomRenderTextureWidth;

    float4 input = LOAD_X(_Source, crt.localTexcoord.xyz, crt.direction);
    float4 color = input;

    if (all(input.rgb > _Threshold * STEP_LENGTH))
        return color;

    float3 n1 = SAMPLE_X(_UVMap, s_point_repeat_sampler, crt.localTexcoord.xyz + offset);

    float minLength = 1e20;
    int k = 0;
    for (int i = -_Radius; i <= _Radius; i++)
    {
        for (int j = -_Radius; j <= _Radius; j++)
#if defined(CRT_3D)
            for (k = -_Radius; k <= _Radius; k++)
#endif
            {
                if (i == 0 && j == 0 && k == 0)
                    continue;
                    
                float l = length(float3(i, j, k) / _Radius);
                if (l > 1.0)
                    continue;

                float3 uvOffset = float3(i, j, k) / float3(_CustomRenderTextureWidth, _CustomRenderTextureHeight, _CustomRenderTextureDepth);
                float4 neighbour = LOAD_X(_Source, crt.localTexcoord.xyz + uvOffset, crt.direction);

                if (all(neighbour.rgb > _Threshold))
                {
                    minLength = min(minLength, l);
                    color = neighbour;
                }
            }
    }

    if (minLength <= 1.0)
        color = lerp(input, color, 1.0 - minLength);

    return color;
}
