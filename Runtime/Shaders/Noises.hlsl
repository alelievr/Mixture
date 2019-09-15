#ifndef NOISES_HLSL
# define NOISES_HLSL

float4 Interpolation_C2_InterpAndDeriv(float2 x) { return x.xyxy * x.xyxy * (x.xyxy * (x.xyxy * (x.xyxy * float2(6.0f, 0.0f).xxyy + float2(-15.0f, 30.0f).xxyy) + float2(10.0f, -60.0f).xxyy) + float2(0.0f, 30.0f).xxyy); }
float3 Interpolation_C2(float3 x) { return x * x * x * (x * (x * 6.0f - 15.0f) + 10.0f); }
float3 Interpolation_C2_Deriv(float3 x) { return x * x * (x * (x * 30.0f - 60.0f) + 30.0f); }

// Generates 2 random numbers for each of the 4 cell corners
void NoiseHash2D(float2 gridcell, out float4 hash_0, out float4 hash_1)
{
    float2 kOffset = float2(26.0f, 161.0f);
    float kDomain = 71.0f;
    float2 kLargeFloats = 1.0f / float2(951.135664f, 642.949883f);

    float4 P = float4(gridcell.xy, gridcell.xy + 1.0f);
    P = P - floor(P * (1.0f / kDomain)) * kDomain;
    P += kOffset.xyxy;
    P *= P;
    P = P.xzxz * P.yyww;
    hash_0 = frac(P * kLargeFloats.x);
    hash_1 = frac(P * kLargeFloats.y);
}

// Generates 3 random numbers for each of the 8 cell corners
void NoiseHash3D(float3 gridcell,
    out float4 lowz_hash_0, out float4 lowz_hash_1, out float4 lowz_hash_2,
    out float4 highz_hash_0, out float4 highz_hash_1, out float4 highz_hash_2)
{
    float2 kOffset = float2(50.0f, 161.0f);
    float kDomain = 69.0f;
    float3 kLargeFloats = float3(635.298681f, 682.357502f, 668.926525f);
    float3 kZinc = float3(48.500388f, 65.294118f, 63.934599f);

    // truncate the domain
    gridcell.xyz = gridcell.xyz - floor(gridcell.xyz * (1.0f / kDomain)) * kDomain;
    float3 gridcell_inc1 = step(gridcell, float3(kDomain, kDomain, kDomain) - 1.5f) * (gridcell + 1.0f);

    // calculate the final hash
    float4 P = float4(gridcell.xy, gridcell_inc1.xy) + kOffset.xyxy;
    P *= P;
    P = P.xzxz * P.yyww;
    float3 lowz_mod = float3(1.0f / (kLargeFloats + gridcell.zzz * kZinc));
    float3 highz_mod = float3(1.0f / (kLargeFloats + gridcell_inc1.zzz * kZinc));
    lowz_hash_0 = frac(P * lowz_mod.xxxx);
    highz_hash_0 = frac(P * highz_mod.xxxx);
    lowz_hash_1 = frac(P * lowz_mod.yyyy);
    highz_hash_1 = frac(P * highz_mod.yyyy);
    lowz_hash_2 = frac(P * lowz_mod.zzzz);
    highz_hash_2 = frac(P * highz_mod.zzzz);
}

float perlinNoise2D(float2 coordinate)
{
    // establish our grid cell and unit position
    float2 i = floor(coordinate);
    float4 f_fmin1 = coordinate.xyxy - float4(i, i + 1.0f);

    // calculate the hash
    float4 hash_x, hash_y;
    NoiseHash2D(i, hash_x, hash_y);

    // calculate the gradient results
    float4 grad_x = hash_x - 0.49999f;
    float4 grad_y = hash_y - 0.49999f;
    float4 norm = rsqrt(grad_x * grad_x + grad_y * grad_y);
    grad_x *= norm;
    grad_y *= norm;
    float4 dotval = (grad_x * f_fmin1.xzxz + grad_y * f_fmin1.yyww);

    // convert our data to a more parallel format
    float3 dotval0_grad0 = float3(dotval.x, grad_x.x, grad_y.x);
    float3 dotval1_grad1 = float3(dotval.y, grad_x.y, grad_y.y);
    float3 dotval2_grad2 = float3(dotval.z, grad_x.z, grad_y.z);
    float3 dotval3_grad3 = float3(dotval.w, grad_x.w, grad_y.w);

    // evaluate common constants
    float3 k0_gk0 = dotval1_grad1 - dotval0_grad0;
    float3 k1_gk1 = dotval2_grad2 - dotval0_grad0;
    float3 k2_gk2 = dotval3_grad3 - dotval2_grad2 - k0_gk0;

    // C2 Interpolation
    float4 blend = Interpolation_C2_InterpAndDeriv(f_fmin1.xy);

    // calculate final noise + deriv
    float3 results = dotval0_grad0
        + blend.x * k0_gk0
        + blend.y * (k1_gk1 + blend.x * k2_gk2);

    results.yz += blend.zw * (float2(k0_gk0.x, k1_gk1.x) + blend.yx * k2_gk2.xx);

    return results * 1.4142135623730950488016887242097f;  // scale to -1.0 -> 1.0 range  *= 1.0/sqrt(0.5)
}

float4 perlinNoise3D(float3 coordinate)
{
    // establish our grid cell and unit position
    float3 i = floor(coordinate);
    float3 f = coordinate - i;
    float3 f_min1 = f - 1.0;

    // calculate the hash
    float4 hashx0, hashy0, hashz0, hashx1, hashy1, hashz1;
    NoiseHash3D(i, hashx0, hashy0, hashz0, hashx1, hashy1, hashz1);

    // calculate the gradients
    float4 grad_x0 = hashx0 - 0.49999f;
    float4 grad_y0 = hashy0 - 0.49999f;
    float4 grad_z0 = hashz0 - 0.49999f;
    float4 grad_x1 = hashx1 - 0.49999f;
    float4 grad_y1 = hashy1 - 0.49999f;
    float4 grad_z1 = hashz1 - 0.49999f;
    float4 norm_0 = rsqrt(grad_x0 * grad_x0 + grad_y0 * grad_y0 + grad_z0 * grad_z0);
    float4 norm_1 = rsqrt(grad_x1 * grad_x1 + grad_y1 * grad_y1 + grad_z1 * grad_z1);
    grad_x0 *= norm_0;
    grad_y0 *= norm_0;
    grad_z0 *= norm_0;
    grad_x1 *= norm_1;
    grad_y1 *= norm_1;
    grad_z1 *= norm_1;

    // calculate the dot products
    float4 dotval_0 = float2(f.x, f_min1.x).xyxy * grad_x0 + float2(f.y, f_min1.y).xxyy * grad_y0 + f.zzzz * grad_z0;
    float4 dotval_1 = float2(f.x, f_min1.x).xyxy * grad_x1 + float2(f.y, f_min1.y).xxyy * grad_y1 + f_min1.zzzz * grad_z1;

    // convert our data to a more parallel format
    float4 dotval0_grad0 = float4(dotval_0.x, grad_x0.x, grad_y0.x, grad_z0.x);
    float4 dotval1_grad1 = float4(dotval_0.y, grad_x0.y, grad_y0.y, grad_z0.y);
    float4 dotval2_grad2 = float4(dotval_0.z, grad_x0.z, grad_y0.z, grad_z0.z);
    float4 dotval3_grad3 = float4(dotval_0.w, grad_x0.w, grad_y0.w, grad_z0.w);
    float4 dotval4_grad4 = float4(dotval_1.x, grad_x1.x, grad_y1.x, grad_z1.x);
    float4 dotval5_grad5 = float4(dotval_1.y, grad_x1.y, grad_y1.y, grad_z1.y);
    float4 dotval6_grad6 = float4(dotval_1.z, grad_x1.z, grad_y1.z, grad_z1.z);
    float4 dotval7_grad7 = float4(dotval_1.w, grad_x1.w, grad_y1.w, grad_z1.w);

    // evaluate common constants
    float4 k0_gk0 = dotval1_grad1 - dotval0_grad0;
    float4 k1_gk1 = dotval2_grad2 - dotval0_grad0;
    float4 k2_gk2 = dotval4_grad4 - dotval0_grad0;
    float4 k3_gk3 = dotval3_grad3 - dotval2_grad2 - k0_gk0;
    float4 k4_gk4 = dotval5_grad5 - dotval4_grad4 - k0_gk0;
    float4 k5_gk5 = dotval6_grad6 - dotval4_grad4 - k1_gk1;
    float4 k6_gk6 = (dotval7_grad7 - dotval6_grad6) - (dotval5_grad5 - dotval4_grad4) - k3_gk3;

    // C2 Interpolation
    float3 blend = Interpolation_C2(f);
    float3 blendDeriv = Interpolation_C2_Deriv(f);

    // calculate final noise + deriv
    float u = blend.x;
    float v = blend.y;
    float w = blend.z;

    float4 result = dotval0_grad0
        + u * (k0_gk0 + v * k3_gk3)
        + v * (k1_gk1 + w * k5_gk5)
        + w * (k2_gk2 + u * (k4_gk4 + v * k6_gk6));

    result.y += dot(float4(k0_gk0.x, k3_gk3.x * v, float2(k4_gk4.x, k6_gk6.x * v) * w), float4(blendDeriv.xxxx));
    result.z += dot(float4(k1_gk1.x, k3_gk3.x * u, float2(k5_gk5.x, k6_gk6.x * u) * w), float4(blendDeriv.yyyy));
    result.w += dot(float4(k2_gk2.x, k4_gk4.x * u, float2(k5_gk5.x, k6_gk6.x * u) * v), float4(blendDeriv.zzzz));

    // normalize
    return result * 1.1547005383792515290182975610039f;		// scale to -1.0 -> 1.0 range    *= 1.0/sqrt(0.75)
}

#define NOISE_TEMPLATE(NAME, COORDINATE_TYPE, RETURN_TYPE, FUNC) \
RETURN_TYPE Generate##NAME##Noise(COORDINATE_TYPE coordinate, float frequency, int octaveCount, float persistence, float lacunarity) \
{ \
    RETURN_TYPE total = 0.0f; \
\
    float amplitude = 1.0f; \
    float totalAmplitude = 0.0f; \
\
    for (int octaveIndex = 0; octaveIndex < octaveCount; octaveIndex++) \
    { \
        total += FUNC(coordinate * frequency) * amplitude; \
        totalAmplitude += amplitude; \
        amplitude *= persistence; \
        frequency *= lacunarity; \
    } \
 \
    return total / totalAmplitude; \
}

NOISE_TEMPLATE(Perlin2D, float2, float3, perlinNoise2D);
NOISE_TEMPLATE(Perlin3D, float3, float4, perlinNoise3D);

#endif