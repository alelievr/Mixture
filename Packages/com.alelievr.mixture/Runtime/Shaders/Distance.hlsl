static float2 offset2D[9] = 
{
    float2(-1, -1),
    float2(-1,  0),
    float2(-1,  1),
    float2( 0, -1),
    float2( 0,  0),
    float2( 0,  1),
    float2( 1, -1),
    float2( 1,  0),
    float2( 1,  1),
};

// STEP_LENGTH needs to be defined to call this function
float4 mixture (v2f_customrendertexture crt) : SV_Target
{
    // The SAMPLE_X macro handles sampling for 2D, 3D and cube textures

    // TODO: non-pot handling
    float3 offset = (1 << STEP_LENGTH) / _CustomRenderTextureWidth;

    float4 input = LOAD_X(_Source, crt.localTexcoord.xyz, crt.direction);
    float4 color = input;

    if (all(input.rgb > _Threshold))
        return color;

    float3 nearest = LOAD_X(_UVMap, crt.localTexcoord.xyz, crt.direction).xyz;

    for (int i = 0; i < 9; i++)
    {
        float3 o = float3(offset2D[i], 0) * offset;
        float3 n1 = SAMPLE_X_SAMPLER(_UVMap, s_point_repeat_sampler, o, 0).xyz;

        // Discard invalid samples
        if (any(n1) < 0)
            continue;
        
        if (length(crt.localTexcoord.xyz - n1) < length(crt.localTexcoord.xyz - nearest))
        {
            nearest = n1;
        }
    }

    return float4(nearest, 0);
}
