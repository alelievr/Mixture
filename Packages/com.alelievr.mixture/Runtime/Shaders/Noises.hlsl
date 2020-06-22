#ifndef NOISES_HLSL
# define NOISES_HLSL

#define EUCLIDEAN_DISTANCE  0
#define MANHATTAN_DISTANCE  1
#define MINKOWSKI_DISTANCE_0_4  2

#ifdef CUSTOM_DISTANCE
# define DISTANCE_ALGORITHM CUSTOM_DISTANCE
#else
# define DISTANCE_ALGORITHM EUCLIDEAN_DISTANCE
#endif

#ifdef CUSTOM_DISTANCE_MULTIPLIER
# define DISTANCE_MULTIPLIER CUSTOM_DISTANCE_MULTIPLIER
#else
# define DISTANCE_MULTIPLIER 1.0
#endif

float MinkowskiDistance(float3 p, float d)
{
    float3 v = abs(p);
    return pow(pow(v.x, d) + pow(v.y, d) + pow(v.z, d), 1.0 / d);
}

float Distance(float3 p)
{
    float d;

    switch (DISTANCE_ALGORITHM)
    {
        case MANHATTAN_DISTANCE:
            d = abs(p.x) + abs(p.y) + abs(p.z); break;
        case MINKOWSKI_DISTANCE_0_4:
            d = MinkowskiDistance(p, 0.4); break;
        case EUCLIDEAN_DISTANCE:
        default:
            d = length(p); break;
    }

    return d * DISTANCE_MULTIPLIER;
}

float Distance(float2 p) { return Distance(float3(p, 0)); }

// Reference noises based on https://github.com/BrianSharpe/GPU-Noise-Lib/blob/master/gpu_noise_lib.glsl

#define NOISE_TEMPLATE(NAME, COORDINATE_TYPE, RETURN_TYPE, FUNC_CALL) \
RETURN_TYPE Generate##NAME##Noise(COORDINATE_TYPE coordinate, float frequency, int octaveCount, float persistence, float lacunarity) \
{ \
    RETURN_TYPE total = 0.0f; \
    if (DISTANCE_ALGORITHM != EUCLIDEAN_DISTANCE) \
        total = 1e20; \
\
    float amplitude = 1.0f; \
    float totalAmplitude = 0.0f; \
\
    for (int octaveIndex = 0; octaveIndex < octaveCount; octaveIndex++) \
    { \
        if (DISTANCE_ALGORITHM != EUCLIDEAN_DISTANCE) \
            total = min(total, FUNC_CALL * amplitude); \
        else \
            total += FUNC_CALL * amplitude; \
        totalAmplitude += amplitude; \
        amplitude *= persistence; \
        frequency *= lacunarity; \
    } \
 \
    return total / ((DISTANCE_ALGORITHM != EUCLIDEAN_DISTANCE) ? 1 : totalAmplitude); \
}

#define RIDGED_NOISE_TEMPLATE(NAME, COORDINATE_TYPE, RETURN_TYPE, FUNC_CALL) \
RETURN_TYPE GenerateRidged##NAME##Noise(COORDINATE_TYPE coordinate, float frequency, int octaveCount, float persistence, float lacunarity) \
{ \
    RETURN_TYPE total = 0.0f; \
    if (DISTANCE_ALGORITHM != EUCLIDEAN_DISTANCE) \
        total = 1e20; \
\
    float amplitude = 1.0f; \
    float totalAmplitude = 0.0f; \
\
    for (int octaveIndex = 0; octaveIndex < octaveCount; octaveIndex++) \
    { \
        if (DISTANCE_ALGORITHM != EUCLIDEAN_DISTANCE) \
            total = abs(min(total, FUNC_CALL * amplitude)); \
        else \
            total += abs(FUNC_CALL * amplitude); \
        totalAmplitude += amplitude; \
        amplitude *= persistence; \
        frequency *= lacunarity; \
    } \
 \
    return total / ((DISTANCE_ALGORITHM != EUCLIDEAN_DISTANCE) ? 1 : totalAmplitude); \
}

// #define CURL_NOISE_2D_TEMPLATE(NAME, FUNC_CALL) \
// float2 Generate##NAME##CurlNoise(float2 coordinate, float frequency, int octaveCount, float persistence, float lacunarity) \
// { \
//     float2 total = float2(0.0f, 0.0f); \
// \
//     float amplitude = 1.0f; \
//     float totalAmplitude = 0.0f; \
// \
//     for (int octaveIndex = 0; octaveIndex < octaveCount; octaveIndex++) \
//     { \
//         float2 derivatives = FUNC_CALL(coordinate * frequency).yz; \
//         total += derivatives * amplitude; \
// \
//         totalAmplitude += amplitude; \
//         amplitude *= persistence; \
//         frequency *= lacunarity; \
//     } \
// \
//     return float2(total.y, -total.x) / totalAmplitude; \
// }

// #define CURL_NOISE_3D_TEMPLATE(NAME, FUNC_CALL) \
// float3 Generate##NAME##CurlNoise(float3 coordinate, float frequency, int octaveCount, float persistence, float lacunarity) \
// { \
//     float2 total[3] = { float2(0.0f, 0.0f), float2(0.0f, 0.0f), float2(0.0f, 0.0f) }; \
// \
//     float amplitude = 1.0f; \
//     float totalAmplitude = 0.0f; \
// \
//     float2 points[3] = \
//     { \
//         coordinate.zy, \
//         coordinate.xz + 100.0f, \
//         coordinate.yx + 200.0f \
//     }; \
// \
//     for (int octaveIndex = 0; octaveIndex < octaveCount; octaveIndex++) \
//     { \
//         for (int i = 0; i < 3; i++) \
//         { \
//             float2 derivatives = FUNC_CALL(points[i] * frequency).yz; \
//             total[i] += derivatives * amplitude; \
//         } \
// \
//         totalAmplitude += amplitude; \
//         amplitude *= persistence; \
//         frequency *= lacunarity; \
//     } \
// \
//     return float3( \
//         (total[2].x - total[1].y), \
//         (total[0].x - total[2].y), \
//         (total[1].x - total[0].y)) / totalAmplitude; \
// }

// TODO: make the rotation a parameter
const float2x2 rMatrix = float2x2( 0.80,  0.60, -0.60,  0.80 );

#define FBM4_TEMPLATE(NAME, COORDINATE_TYPE, FUNC) \
float Generate##NAME##_FBM4(COORDINATE_TYPE p) \
{ \
    float f = 0.0; \
 \
    f += 0.5000 * (-1.0 + 2.0 * FUNC(p)); p = mul(rMatrix, p) * 2.02; \
    f += 0.2500 * (-1.0 + 2.0 * FUNC(p)); p = mul(rMatrix, p) * 2.03; \
    f += 0.1250 * (-1.0 + 2.0 * FUNC(p)); p = mul(rMatrix, p) * 2.01; \
    f += 0.0625 * (-1.0 + 2.0 * FUNC(p)); \
 \
    return f / 0.9375; \
} \
 \
float2 Generate##NAME##_FBM4_2(float2 p) \
{ \
    return float2( \
        Generate##NAME##_FBM4(p + -5.92), \
        Generate##NAME##_FBM4(p + 3.88) \
    ); \
}

#define FBM6_TEMPLATE(NAME, COORDINATE_TYPE, FUNC) \
float Generate##NAME##_FBM6(COORDINATE_TYPE p) \
{ \
    float f = 0.0; \
 \
    f += 0.500000 * FUNC(p); p = mul(rMatrix, p) * 2.02; \
    f += 0.250000 * FUNC(p); p = mul(rMatrix, p) * 2.03; \
    f += 0.125000 * FUNC(p); p = mul(rMatrix, p) * 2.01; \
    f += 0.062500 * FUNC(p); p = mul(rMatrix, p) * 2.04; \
    f += 0.031250 * FUNC(p); p = mul(rMatrix, p) * 2.01; \
    f += 0.015625 * FUNC(p); \
 \
    return f / 0.96875; \
} \
COORDINATE_TYPE Generate##NAME##_FBM6_2(COORDINATE_TYPE p) \
{ \
    return COORDINATE_TYPE( \
        Generate##NAME##_FBM6(p + 1.99), \
        Generate##NAME##_FBM6(p + -4.18) \
    ); \
}

// Utils:

#ifdef UNITY_CUSTOM_TEXTURE_INCLUDED

float3 RandomOffset3(int seed)
{
    float3 v = float3(-6.747, 8.488, 3.584) * seed;

    v %= 5000;

    return v;
}

float3 GetNoiseUVs(v2f_customrendertexture i, float3 customUvs, int seed)
{
    float3 offset = RandomOffset3(seed);

#ifdef USE_CUSTOM_UV
    return customUvs + offset;
#else
    return GetDefaultUVs(i) + offset;
#endif
}
#endif

// White noise:

float WhiteNoise(float3 uvs)
{
    float3 smallValue = sin(uvs);
    float random = dot(smallValue, float3(12.9898, 78.233, 37.719));
    random = frac(sin(random) * 143758.5453);
    return random;
}

// Perlin:

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

float3 perlinNoise2D(float2 coordinate)
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

float2 modulo(float2 divident, float2 divisor)
{
    float2 positiveDivident = divident % divisor + divisor;
    return positiveDivident % divisor;
}

float3 modulo(float3 divident, float3 divisor)
{
    float3 positiveDivident = divident % divisor + divisor;
    return positiveDivident % divisor;
}

float easeIn(float interpolator){
    return interpolator * interpolator;
}

float easeOut(float interpolator){
    return 1 - easeIn(1 - interpolator);
}

float easeInOut(float interpolator){
    float easeInValue = easeIn(interpolator);
    float easeOutValue = easeOut(interpolator);
    return lerp(easeInValue, easeOutValue, interpolator);
}

float4 taylorInvSqrt(float4 r)
{
    return 1.79284291400159 - 0.85373472095314 * r;
}

float4 mod289(float4 x)
{
    return x - floor(x * (1.0 / 289.0)) * 289.0;
}

float4 permute(float4 x)
{
    return mod289(((x * 34) + 1) * x);
}

float2 fade(float2 t) { return (t * t * t) * (t * (t * 6 - 15) + 10); }
float3 fade(float3 t) { return (t * t * t) * (t * (t * 6 - 15) + 10); }

float3 mod(float3 x, float3 y) { return x - y * floor(x / y); }
float4 mod(float4 x, float4 y) { return x - y * floor(x / y); }

float tiledPerlinNoise2D(float2 coordinate, float2 period)
{
    float4 Pi = floor(float4(coordinate.x, coordinate.y, coordinate.x, coordinate.y)) + float4(0.0, 0.0, 1.0, 1.0);
    float4 Pf = frac(float4(coordinate.x, coordinate.y, coordinate.x, coordinate.y)) - float4(0.0, 0.0, 1.0, 1.0);
    Pi = mod(Pi, float4(period.x, period.y, period.x, period.y)); // To create noise with explicit period
    Pi = mod(Pi, 289); // To avoid truncation effects in permutation
    float4 ix = float4(Pi.x, Pi.z, Pi.x, Pi.z);
    float4 iy = float4(Pi.y, Pi.y, Pi.w, Pi.w);
    float4 fx = float4(Pf.x, Pf.z, Pf.x, Pf.z);
    float4 fy = float4(Pf.y, Pf.y, Pf.w, Pf.w);

    float4 i = permute(permute(ix) + iy);

    float4 gx = 2 * frac(i / float(41)) - float(1);
    float4 gy = abs(gx) - 0.5;
    float4 tx = floor(gx + 0.5);
    gx = gx - tx;

    float2 g00 = float2(gx.x, gy.x);
    float2 g10 = float2(gx.y, gy.y);
    float2 g01 = float2(gx.z, gy.z);
    float2 g11 = float2(gx.w, gy.w);

    float4 norm = taylorInvSqrt(float4(dot(g00, g00), dot(g01, g01), dot(g10, g10), dot(g11, g11)));
    g00 *= norm.x;
    g01 *= norm.y;
    g10 *= norm.z;
    g11 *= norm.w;

    float n00 = dot(g00, float2(fx.x, fy.x));
    float n10 = dot(g10, float2(fx.y, fy.y));
    float n01 = dot(g01, float2(fx.z, fy.z));
    float n11 = dot(g11, float2(fx.w, fy.w));

    float2 fade_xy = fade(float2(Pf.x, Pf.y));
    float2 n_x = lerp(float2(n00, n01), float2(n10, n11), fade_xy.x);
    float n_xy = lerp(n_x.x, n_x.y, fade_xy.y);
    return float(2.3) * n_xy;
}

float tiledPerlinNoise3D(float3 coordinate, float3 period)
{
    float3 Pi0 = mod(floor(coordinate), period); // Integer part, modulo period
    float3 Pi1 = mod(Pi0 + 1, period); // Integer part + 1, mod period
    Pi0 = mod(Pi0, 289);
    Pi1 = mod(Pi1, 289);
    float3 Pf0 = frac(coordinate); // Fractional part for interpolation
    float3 Pf1 = Pf0 - 1; // Fractional part - 1.0
    float4 ix = float4(Pi0.x, Pi1.x, Pi0.x, Pi1.x);
    float4 iy = float4(Pi0.y, Pi0.y, Pi1.y, Pi1.y);
    float4 iz0 = Pi0.z;
    float4 iz1 = Pi1.z;

    float4 ixy = permute(permute(ix) + iy);
    float4 ixy0 = permute(ixy + iz0);
    float4 ixy1 = permute(ixy + iz1);

    float4 gx0 = ixy0 / 7;
    float4 gy0 = frac(floor(gx0) / 7) - 0.5;
    gx0 = frac(gx0);
    float4 gz0 = 0.5 - abs(gx0) - abs(gy0);
    float4 sz0 = step(gz0, 0);
    gx0 -= sz0 * (step(0, gx0) - 0.5);
    gy0 -= sz0 * (step(0, gy0) - 0.5);

    float4 gx1 = ixy1 / 7;
    float4 gy1 = frac(floor(gx1) / 7) - 0.5;
    gx1 = frac(gx1);
    float4 gz1 = 0.5 - abs(gx1) - abs(gy1);
    float4 sz1 = step(gz1, 0);
    gx1 -= sz1 * (step(0, gx1) - 0.5);
    gy1 -= sz1 * (step(0, gy1) - 0.5);

    float3 g000 = float3(gx0.x, gy0.x, gz0.x);
    float3 g100 = float3(gx0.y, gy0.y, gz0.y);
    float3 g010 = float3(gx0.z, gy0.z, gz0.z);
    float3 g110 = float3(gx0.w, gy0.w, gz0.w);
    float3 g001 = float3(gx1.x, gy1.x, gz1.x);
    float3 g101 = float3(gx1.y, gy1.y, gz1.y);
    float3 g011 = float3(gx1.z, gy1.z, gz1.z);
    float3 g111 = float3(gx1.w, gy1.w, gz1.w);

    float4 norm0 = taylorInvSqrt(float4(dot(g000, g000), dot(g010, g010), dot(g100, g100), dot(g110, g110)));
    g000 *= norm0.x;
    g010 *= norm0.y;
    g100 *= norm0.z;
    g110 *= norm0.w;
    float4 norm1 = taylorInvSqrt(float4(dot(g001, g001), dot(g011, g011), dot(g101, g101), dot(g111, g111)));
    g001 *= norm1.x;
    g011 *= norm1.y;
    g101 *= norm1.z;
    g111 *= norm1.w;

    float n000 = dot(g000, Pf0);
    float n100 = dot(g100, float3(Pf1.x, Pf0.y, Pf0.z));
    float n010 = dot(g010, float3(Pf0.x, Pf1.y, Pf0.z));
    float n110 = dot(g110, float3(Pf1.x, Pf1.y, Pf0.z));
    float n001 = dot(g001, float3(Pf0.x, Pf0.y, Pf1.z));
    float n101 = dot(g101, float3(Pf1.x, Pf0.y, Pf1.z));
    float n011 = dot(g011, float3(Pf0.x, Pf1.y, Pf1.z));
    float n111 = dot(g111, Pf1);

    float3 fade_xyz = fade(Pf0);
    float4 n_z = lerp(float4(n000, n100, n010, n110), float4(n001, n101, n011, n111), fade_xyz.z);
    float2 n_yz = lerp(float2(n_z.x, n_z.y), float2(n_z.z, n_z.w), fade_xyz.y);
    float n_xyz = lerp(n_yz.x, n_yz.y, fade_xyz.x);
    return 2.2 * n_xyz;
}

#ifdef _TILINGMODE_TILED

NOISE_TEMPLATE(Perlin2D, float2, float, tiledPerlinNoise2D(coordinate * frequency, frequency));
NOISE_TEMPLATE(Perlin3D, float3, float, tiledPerlinNoise3D(coordinate * frequency, frequency));
RIDGED_NOISE_TEMPLATE(Perlin2D, float2, float, tiledPerlinNoise2D(coordinate * frequency, frequency));
RIDGED_NOISE_TEMPLATE(Perlin3D, float3, float, tiledPerlinNoise3D(coordinate * frequency, frequency));

#else

NOISE_TEMPLATE(Perlin2D, float2, float3, perlinNoise2D(coordinate * frequency));
NOISE_TEMPLATE(Perlin3D, float3, float4, perlinNoise3D(coordinate * frequency));
RIDGED_NOISE_TEMPLATE(Perlin2D, float2, float3, perlinNoise2D(coordinate * frequency));
RIDGED_NOISE_TEMPLATE(Perlin3D, float3, float4, perlinNoise3D(coordinate * frequency));

#endif

// CURL_NOISE_2D_TEMPLATE(Perlin2D, perlinNoise2D);
// CURL_NOISE_3D_TEMPLATE(Perlin3D, perlinNoise2D);

// Cellular:

// Convert a 0.0->1.0 sample to a -1.0->1.0 sample weighted towards the extremes
float4 CellularWeightSamples(float4 samples)
{
    samples = samples * 2.0f - 1.0f;
    //return (1.0 - samples * samples) * sign(samples);	// square
    return (samples * samples * samples) - sign(samples);	// cubic (even more variance)
}

// Credits: https://www.ronja-tutorials.com/2018/10/06/tiling-noise.html
float rand3dTo1d(float3 value, float3 dotDir = float3(12.9898, 78.233, 37.719))
{
	//make value smaller to avoid artefacts
	float3 smallValue = sin(value);
	//get scalar value from 3d vector
	float random = dot(smallValue, dotDir);
	//make value more random by making it bigger and then taking the factional part
	random = frac(sin(random) * 143758.5453);
	return random;
}

float3 rand3dTo3d(float3 value)
{
	return float3(
		rand3dTo1d(value, float3(12.989, 78.233, 37.719)),
		rand3dTo1d(value, float3(39.346, 11.135, 83.155)),
		rand3dTo1d(value, float3(73.156, 52.235, 09.151))
	);
}

float rand2dTo1d(float2 value, float2 dotDir = float2(12.9898, 78.233))
{
	float2 smallValue = sin(value);
	float random = dot(smallValue, dotDir);
	random = frac(sin(random) * 143758.5453);
	return random;
}

float2 rand2dTo2d(float2 value)
{
	return float2(
		rand2dTo1d(value, float2(12.989, 78.233)),
		rand2dTo1d(value, float2(39.346, 11.135))
	);
}

float3 tiledCellularNoise2D(float2 coordinate, float2 period)
{
    float2 baseCell = floor(coordinate);

    //first pass to find the closest cell
    float minDistToCell = 10;
    float2 toClosestCell;
    float2 closestCell;
    [unroll]
    for(int x1=-1; x1<=1; x1++)
    {
        [unroll]
        for(int y1=-1; y1<=1; y1++)
        {
            float2 cell = baseCell + float2(x1, y1);
            float2 tiledCell = modulo(cell, period);
            float2 cellPosition = cell + rand2dTo2d(tiledCell);
            float2 toCell = cellPosition - coordinate;
            float distToCell = Distance(toCell);

            if(distToCell < minDistToCell)
            {
                minDistToCell = distToCell;
                closestCell = cell;
                toClosestCell = toCell;
            }
        }
    }

    //second pass to find the distance to the closest edge
    float minEdgeDistance = 10;
    [unroll]
    for(int x2=-1; x2<=1; x2++)
    {
        [unroll]
        for(int y2=-1; y2<=1; y2++)
        {
            float2 cell = baseCell + float2(x2, y2);
            float2 tiledCell = modulo(cell, period);
            float2 cellPosition = cell + rand2dTo2d(tiledCell);
            float2 toCell = cellPosition - coordinate;

            float2 diffToClosestCell = abs(closestCell - cell);
            bool isClosestCell = diffToClosestCell.x + diffToClosestCell.y < 0.1;
            if(!isClosestCell)
            {
                float2 toCenter = (toClosestCell + toCell) * 0.5;
                float2 cellDifference = normalize(toCell - toClosestCell);
                float edgeDistance = dot(toCenter, cellDifference);
                minEdgeDistance = min(minEdgeDistance, edgeDistance);
            }
        }
    }

    float random = rand2dTo1d(closestCell);
    return float3(pow(minDistToCell, 2.2), random, minEdgeDistance); // Gamma convertion
}

float3 tiledCellularNoise3D(float3 coordinate, float3 period)
{
    float3 baseCell = floor(coordinate);

    //first pass to find the closest cell
    float minDistToCell = 10;
    float3 toClosestCell;
    float3 closestCell;
    [unroll]
    for(int x1=-1; x1<=1; x1++)
    {
        [unroll]
        for(int y1=-1; y1<=1; y1++)
        {
            [unroll]
            for(int z1=-1; z1<=1; z1++)
            {
                float3 cell = baseCell + float3(x1, y1, z1);
                float3 tiledCell = modulo(cell, period);
                float3 cellPosition = cell + rand3dTo3d(tiledCell);
                float3 toCell = cellPosition - coordinate;
                float distToCell = Distance(toCell);
                if(distToCell < minDistToCell)
                {
                    minDistToCell = distToCell;
                    closestCell = cell;
                    toClosestCell = toCell;
                }
            }
        }
    }

    //second pass to find the distance to the closest edge
    float minEdgeDistance = 10;
    [unroll]
    for(int x2=-1; x2<=1; x2++)
    {
        [unroll]
        for(int y2=-1; y2<=1; y2++)
        {
            [unroll]
            for(int z2=-1; z2<=1; z2++)
            {
                float3 cell = baseCell + float3(x2, y2, z2);
                float3 tiledCell = modulo(cell, period);
                float3 cellPosition = cell + rand3dTo3d(tiledCell);
                float3 toCell = cellPosition - coordinate;

                float3 diffToClosestCell = abs(closestCell - cell);
                bool isClosestCell = diffToClosestCell.x + diffToClosestCell.y + diffToClosestCell.z < 0.1;
                if(!isClosestCell)
                {
                    float3 toCenter = (toClosestCell + toCell) * 0.5;
                    float3 cellDifference = normalize(toCell - toClosestCell);
                    float edgeDistance = dot(toCenter, cellDifference);
                    minEdgeDistance = min(minEdgeDistance, edgeDistance);
                }
            }
        }
    }

    float random = rand3dTo1d(closestCell);
    return float3(pow(minDistToCell, 2.2), random, minEdgeDistance);
}

float3 GenerateCellularNoise2D(float2 coordinate) { return tiledCellularNoise2D(coordinate, float2(100000, 100000)); }
float3 GenerateCellularNoise3D(float3 coordinate) { return tiledCellularNoise3D(coordinate, float3(100000, 100000, 100000)); }
float3 GenerateRidgedCellularNoise2D(float2 coordinate) { return tiledCellularNoise2D(coordinate, float2(100000, 100000)) * 2 - 1; }
float3 GenerateRidgedCellularNoise3D(float3 coordinate) { return tiledCellularNoise3D(coordinate, float3(100000, 100000, 100000)) * 2 - 1; }

float3 ridgedTiledCellularNoise2D(float2 coordinate, float2 period) { return tiledCellularNoise2D(coordinate, period) * 2 - 1; }
float3 ridgedTiledCellularNoise3D(float3 coordinate, float3 period) { return tiledCellularNoise3D(coordinate, period) * 2 - 1; }

#ifdef _TILINGMODE_TILED

NOISE_TEMPLATE(Cellular2D, float2, float3, tiledCellularNoise2D(coordinate * frequency, frequency));
NOISE_TEMPLATE(Cellular3D, float3, float3, tiledCellularNoise3D(coordinate * frequency, frequency));
RIDGED_NOISE_TEMPLATE(Cellular2D, float2, float3, ridgedTiledCellularNoise2D(coordinate * frequency, frequency));
RIDGED_NOISE_TEMPLATE(Cellular3D, float3, float3, ridgedTiledCellularNoise3D(coordinate * frequency, frequency));

#else

NOISE_TEMPLATE(Cellular2D, float2, float3, GenerateCellularNoise2D(coordinate * frequency));
NOISE_TEMPLATE(Cellular3D, float3, float3, GenerateCellularNoise3D(coordinate * frequency));
RIDGED_NOISE_TEMPLATE(Cellular2D, float2, float3, GenerateRidgedCellularNoise2D(coordinate * frequency));
RIDGED_NOISE_TEMPLATE(Cellular3D, float3, float3, GenerateRidgedCellularNoise3D(coordinate * frequency));

#endif

// CURL_NOISE_2D_TEMPLATE(Cellular2D, GenerateCellularNoise2D);
// CURL_NOISE_3D_TEMPLATE(Cellular3D, GenerateCellularNoise2D);

// FBMs:

float fbmPerlinNoise2D(float2 c) { return perlinNoise2D(c).x; }

FBM4_TEMPLATE(Perlin2D, float2, fbmPerlinNoise2D);
FBM6_TEMPLATE(Perlin2D, float2, fbmPerlinNoise2D);

float GeneratePerlin2D_FBM(float2 coordinate)
{
    float2 uvSet1 = 0.5 * GeneratePerlin2D_FBM4_2(coordinate) + 0.5;

    float2 n = GeneratePerlin2D_FBM6_2( 4.0*uvSet1 );

    float2 p = coordinate + 2.0*n;

    float f = 0.5 + 0.5 * GeneratePerlin2D_FBM4( 2.0*p );

    return f;
}

// Tilable noises:



#endif