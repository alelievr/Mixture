#ifndef MIXTURE_BLENDING
#define MIXTURE_BLENDING

float4 Blend(float4 source, float4 target, float4 mask, uint blendMode)
{
	float4 tmp, result1, result2, zeroOrOne;
	switch (blendMode)
	{
		default:
		case 0: // Normal
			return lerp(source, target, mask);
		case 1: // Min 
			return lerp(source, min(source, target), mask);
		case 2: // Max
			return lerp(source, max(source, target), mask);
		case 3: // Burn
			tmp =  1.0 - (1.0 - target)/source;
			return lerp(source, tmp, mask);
		case 4: // Darken
			tmp = min(target, source);
			return lerp(source, tmp, mask);
		case 5: // Difference
			tmp = abs(target - source);
			return lerp(source, tmp, mask);
		case 6: // Dodge
			tmp = source / (1.0 - target);
			return lerp(source, tmp, mask);
		case 7: // Divide
			tmp = source / (target + 0.000000000001);
			return lerp(source, tmp, mask);
		case 8: // Exclusion
			tmp = target + source - (2.0 * target * source);
			return lerp(source, tmp, mask);
		case 9: // HardLight
			result1 = 1.0 - 2.0 * (1.0 - source) * (1.0 - target);
			result2 = 2.0 * source * target;
			zeroOrOne = step(target, 0.5);
			tmp = result2 * zeroOrOne + (1 - zeroOrOne) * result1;
			return lerp(source, tmp, mask);
		case 10: // HardMix
			tmp = step(1 - source, target);
			return lerp(source, tmp, mask);
		case 11: // Lighten
			tmp = max(target, source);
			return lerp(source, tmp, mask);
		case 12: // LinearBurn
			tmp = source + target - 1.0;
			return lerp(source, tmp, mask);
		case 13: // LinearDodge
			tmp = source + target;
			return lerp(source, tmp, mask);
		case 14: // LinearLight
			tmp = target < 0.5 ? max(source + (2 * target) - 1, 0) : min(source + 2 * (target - 0.5), 1);
			return lerp(source, tmp, mask);
		case 15: // LinearLightAddSub
			tmp = target + 2.0 * source - 1.0;
			return lerp(source, tmp, mask);
		case 16: // Multiply
			tmp = source * target;
			return lerp(source, tmp, mask);
		case 17: // Negation
			tmp = 1.0 - abs(1.0 - target - source);
			return lerp(source, tmp, mask);
		case 18: // Overlay
			result1 = 1.0 - 2.0 * (1.0 - source) * (1.0 - target);
			result2 = 2.0 * source * target;
			zeroOrOne = step(source, 0.5);
			tmp = result2 * zeroOrOne + (1 - zeroOrOne) * result1;
			return lerp(source, tmp, mask);
		case 19: // PinLight
			float4 check = step (0.5, target);
			result1 = check * max(2.0 * (source - 0.5), target);
			tmp = result1 + (1.0 - check) * min(2.0 * source, target);
			return lerp(source, tmp, mask);
		case 20: // Screen
			tmp = 1.0 - (1.0 - target) * (1.0 - source);
			return lerp(source, tmp, mask);
		case 21: // SoftLight
			result1 = 2.0 * source * target + source * source * (1.0 - 2.0 * target);
			result2 = sqrt(source) * (2.0 * target - 1.0) + 2.0 * source * (1.0 - target);
			zeroOrOne = step(0.5, target);
			tmp = result2 * zeroOrOne + (1 - zeroOrOne) * result1;
			return lerp(source, tmp, mask);
		case 22: // Subtract
			tmp = source - target;
			return lerp(source, tmp, mask);
		case 23: // VividLight
			result1 = 1.0 - (1.0 - target) / (2.0 * source);
			result2 = target / (2.0 * (1.0 - source));
			zeroOrOne = step(0.5, source);
			tmp = result2 * zeroOrOne + (1 - zeroOrOne) * result1;
			return lerp(source, tmp, mask);
		case 24: // Transparent
			return source.a * source + (1 - source.a) * target;
	}
}

#endif