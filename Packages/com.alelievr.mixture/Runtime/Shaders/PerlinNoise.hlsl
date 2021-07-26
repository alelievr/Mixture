#ifndef PERLIN_NOISE
# define PERLIN_NOISE

#include "Packages/com.alelievr.mixture/Runtime/Shaders/NoiseUtils.hlsl"

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

float3 perlinNoise2D(float2 coordinate, float seed)
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

float4 perlinNoise3D(float3 coordinate, float seed)
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

float3 tiledPerlinNoise2D(float2 pos, float2 scale, float seed)
{
    float rotation = 0.0; // We can expose this if needed

    float2 sinCos = float2(sin(rotation), cos(rotation));
    float2x2 transform = float2x2(sinCos.y, sinCos.x, sinCos.x, sinCos.y);

    // based on Modifications to Classic Perlin Noise by Brian Sharpe: https://archive.is/cJtlS
    pos *= scale;
    float4 i = floor(pos).xyxy + float2(0.0, 1.0).xxyy;
    float4 f = (pos.xyxy - i.xyxy) - float2(0.0, 1.0).xxyy;
    i = mod(i, scale.xyxy) + seed;

    // grid gradients
    float4 gradientX, gradientY;
    multiHash2D(i, gradientX, gradientY);
    gradientX -= 0.49999;
    gradientY -= 0.49999;

    // transform gradients
    float4 mt = float4(transform);
    float4 rg = float4(gradientX.x, gradientY.x, gradientX.y, gradientY.y);
    rg = rg.xxzz * mt.xyxy + rg.yyww * mt.zwzw;
    gradientX.xy = rg.xz;
    gradientY.xy = rg.yw;

    rg = float4(gradientX.z, gradientY.z, gradientX.w, gradientY.w);
    rg = rg.xxzz * mt.xyxy + rg.yyww * mt.zwzw;
    gradientX.zw = rg.xz;
    gradientY.zw = rg.yw;

    // perlin surflet
    float4 gradients = rsqrt(gradientX * gradientX + gradientY * gradientY) * (gradientX * f.xzxz + gradientY * f.yyww);
    float4 m = f * f;
    m = m.xzxz + m.yyww;
    m = max(1.0 - m, 0.0);
    float4 m2 = m * m;
    float4 m3 = m * m2;
    // compute the derivatives
    float4 m2Gradients = -6.0 * m2 * gradients;
    float2 grad = float2(dot(m2Gradients, f.xzxz), dot(m2Gradients, f.yyww)) + float2(dot(m3, gradientX), dot(m3, gradientY));
    // sum the surflets and normalize: 1.0 / 0.75^3
    return float3(dot(m3, gradients), grad) * 2.3703703703703703703703703703704;
}

// In case we need derivatives in tiled perlin noise 3D one day
// float smoothstepDeriv(float t) 
// { 
//     return t * (6 - 6 * t); 
// }

// float smoothstep(float t) 
// { 
//     return t * t * (3 - 2 * t); 
// } 

float tiledPerlinNoise3D(float3 coordinate, float3 period, float seed)
{
    float3 Pi0 = mod(floor(coordinate), period); // Integer part, modulo period
    float3 Pi1 = mod(Pi0 + 1, period); // Integer part + 1, mod period
    Pi0 += seed;
    Pi1 += seed;
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

    // float u = smoothstep(Pf1.x); 
    // float v = smoothstep(Pf1.y); 
    // float w = smoothstep(Pf1.z); 
 
    // float du = smoothstepDeriv(Pf1.x); 
    // float dv = smoothstepDeriv(Pf1.y); 
    // float dw = smoothstepDeriv(Pf1.z); 

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

    // // Calculate derivatives: https://www.scratchapixel.com/lessons/procedural-generation-virtual-worlds/perlin-noise-part-2/perlin-noise-computing-derivatives
    // float k0 = (n100 - n000); 
    // float k1 = (n010 - n000); 
    // float k2 = (n001 - n000); 
    // float k3 = (n000 + n110 - n100 - n010); 
    // float k4 = (n000 + n101 - n100 - n001); 
    // float k5 = (n000 + n011 - n010 - n001); 
    // float k6 = (n100 + n010 + n001 + n111 - n000 - n110 - n101 - n011); 
 
    // float3 derivs;
    // derivs.x = du *(k0 + v * k3 + w * k4 + v * w * k6); 
    // derivs.y = dv *(k1 + u * k3 + w * k5 + u * w * k6); 
    // derivs.z = dw *(k2 + u * k4 + v * k5 + u * v * k6); 

    float3 fade_xyz = fade(Pf0);
    float4 n_z = lerp(float4(n000, n100, n010, n110), float4(n001, n101, n011, n111), fade_xyz.z);
    float2 n_yz = lerp(float2(n_z.x, n_z.y), float2(n_z.z, n_z.w), fade_xyz.y);
    float n_xyz = lerp(n_yz.x, n_yz.y, fade_xyz.x);
    return 2.2 * n_xyz;
}

#ifdef _TILINGMODE_TILED

NOISE_TEMPLATE(Perlin2D, float2, float3, tiledPerlinNoise2D(coordinate, frequency, seed));
NOISE_TEMPLATE(Perlin3D, float3, float, tiledPerlinNoise3D(coordinate * frequency, frequency, seed));
RIDGED_NOISE_TEMPLATE(Perlin2D, float2, float3, tiledPerlinNoise2D(coordinate, frequency, seed));
RIDGED_NOISE_TEMPLATE(Perlin3D, float3, float, tiledPerlinNoise3D(coordinate * frequency, frequency, seed));

CURL_NOISE_2D_TEMPLATE(Perlin2D, tiledPerlinNoise2D(coordinate, frequency, seed));
CURL_NOISE_3D_TEMPLATE(Perlin3D, tiledPerlinNoise2D(coordinate, frequency, seed));

#else

NOISE_TEMPLATE(Perlin2D, float2, float3, perlinNoise2D(coordinate * frequency, seed));
NOISE_TEMPLATE(Perlin3D, float3, float4, perlinNoise3D(coordinate * frequency, seed));
RIDGED_NOISE_TEMPLATE(Perlin2D, float2, float3, perlinNoise2D(coordinate * frequency, seed));
RIDGED_NOISE_TEMPLATE(Perlin3D, float3, float4, perlinNoise3D(coordinate * frequency, seed));

CURL_NOISE_2D_TEMPLATE(Perlin2D, perlinNoise2D(coordinate * frequency, seed));
CURL_NOISE_3D_TEMPLATE(Perlin3D, perlinNoise2D(coordinate * frequency, seed));

#endif


#endif