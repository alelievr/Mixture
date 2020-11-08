# Warp
![Mixture.Warp](../../images/Mixture.Warp.png)
## Inputs
Port Name | Description
--- | ---
Input | 
Gradient | 
Intensity | 

## Output
Port Name | Description
--- | ---
output | 

## Description
Distort the input texture using a height map.
Internally this node converts the height map into a normal map and use it to distort the UVs to sample the input texture.

Please note that this node only support Texture2D dimension(s).
