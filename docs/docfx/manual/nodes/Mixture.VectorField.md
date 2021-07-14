# Vector Field
![Mixture.VectorField](../../images/Mixture.VectorField.png)
## Inputs
Port Name | Description
--- | ---
UV | 
Multiplier | 

## Output
Port Name | Description
--- | ---
Out | 

## Description
Generates a vector field using presets to achieve different patterns. The mode property is used to control the pattern to use for the vector field.
Currently these patterns are implemented:
- Direction: generates an uniform vector field with a direction
- Circular: generates a vector field rotating around the middle of the texture
- Stripes: generates alternated stripes of direction vectors
- Turbulence: perlin noise based turbulence vector field.

This node works great in combination with the fluid simulation nodes.

