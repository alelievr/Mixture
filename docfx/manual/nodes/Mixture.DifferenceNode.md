# Difference
![Mixture.DifferenceNode](../../images/Mixture.DifferenceNode.png)
## Inputs
Port Name | Description
--- | ---
A | 
B | 

## Output
Port Name | Description
--- | ---
Out | 

## Description
The Difference Node can be used to detect differences between two textures. You can choose between these modes:
- Error Diff, it performs an abs(A - B) operation between the two textures, you have a multiplier to help you detect faint details.
- Perceptual Diff, this one only works op color (no alpha) and will perform the difference operation in a perceptual color space (JzAzBz).
- Swap, let you swap between the two textures
- Onion Skin, let you interpolate between the two textures using a slider.

