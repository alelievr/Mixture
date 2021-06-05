#if !defined(TEXTURE_TYPE) || !defined(RW_TEXTURE_TYPE) || !defined(FLOAT_X) || !defined(INT_X) || !defined(SWIZZLE_X)
#error Fluid simulation include template not correctly defined!
#endif

#ifndef SECOND_PASS
FLOAT_X GetAdvectedPosTexCoords(TEXTURE_TYPE<FLOAT_X> velocity, FLOAT_X pos, INT_X id, float deltaTime, FLOAT_X size)
{
    pos -= deltaTime * velocity[id];

    // position to UV
    return pos / size + 0.5 / size;
}
#endif

void AdvectBuffer(TEXTURE_TYPE<FLOAT_X_OR_FLOAT> source, TEXTURE_TYPE<FLOAT_X> velocity, RW_TEXTURE_TYPE<FLOAT_X_OR_FLOAT> destination, INT_X id, float dissipation, float deltaTime, FLOAT_X size)
{
	FLOAT_X uv = GetAdvectedPosTexCoords(velocity, id, id, deltaTime, size);

   	destination[id] = source.SampleLevel(s_linear_clamp_sampler, uv, 0) * dissipation;
}
