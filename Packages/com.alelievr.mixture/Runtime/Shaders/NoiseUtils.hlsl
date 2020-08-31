#ifndef NOISE_UTILS
# define NOISE_UTILS

float GenerateNoise(v2f_customrendertexture i, int seed);

// This function requires GenerateNoise(v2f_customrendertexture i, int seed) to be defined
float4 GenerateNoiseForChannels(v2f_customrendertexture i, int channels, int seed)
{
    switch (channels)
    {
        case 0: // RRRR
            return GenerateNoise(i, seed).rrrr;
        case 1: // R
            return float4(GenerateNoise(i, seed), 0, 0, 1);
        case 2: // RG
            return float4(GenerateNoise(i, seed), GenerateNoise(i, seed + 42), 0, 1);
        case 3: // RGB
            return float4(GenerateNoise(i, seed), GenerateNoise(i, seed + 42), GenerateNoise(i, seed - 69), 1);
        case 4: // RGBA
            return float4(GenerateNoise(i, seed), GenerateNoise(i, seed + 42), GenerateNoise(i, seed - 69), GenerateNoise(i, seed + 5359));
        default:
            return GenerateNoise(i, seed).rrrr;
    }
}

#endif