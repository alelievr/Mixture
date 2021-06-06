# Polygon 2D
![Mixture.Polygon2D](../../images/Mixture.Polygon2D.png)
## Inputs
Port Name | Description
--- | ---
UV | 
Inner Color | Color inside the polygon
Outer Color | Color outside of the polygon
Side Count | Number of sides of the polygon, can be a non integer value
Size | Size of the polygon
Smooth | Smooth the polygon edges and creates a gradient between the color inside and outside of the polygon

## Output
Port Name | Description
--- | ---
Out | 

## Description
This node is the base node of all shader operations, it allows you to create a node with a custom behavior by putting a shader in the Shader field.
Note that the shader must be compatible with Custom Render Textures, otherwise it won't work. If you have a doubt you can create a new shader by pressing the button "New Shader".

The node will automatically reflect the shader properties as inputs that you'll be able to connect to other nodes.
This can be especially useful to prototype a new node or just add something that wasn't in the node Library.

For more information, you can check the [Shader Nodes](../ShaderNodes.md) documentation page.

Please note that this node only support Texture2D dimension(s).
