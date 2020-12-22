# Clamp
![Mixture.ClampNode](../../images/Mixture.ClampNode.png)
## Inputs
Port Name | Description
--- | ---
Source | 
Min | 
Max | 

## Output
Port Name | Description
--- | ---
Out | 

## Description
Clamp the input texture values. Note that the clamp is executed for each channel of the texture following this forumla:
```
_Output.rgba = clamp(_Input.rgba, _Min, _Max);
```

