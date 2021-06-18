# Known Issues

## Some node are empty after an upgrade of package or a new install

Sometimes, it happens that a node ceases to work because of an upgrade of the project or a package. This is likely related to the ShaderGraph embedded in Mixture, for more information you can see this issue: https://github.com/alelievr/Mixture/issues/27

The best way to fix that issue is to reimport the **Mixture** folder under **Packages** in the **Project View**.

## High memory consumption

Mixture processing comes with a high memory cost, especially when using 4k textures in the graph. This is because every node holds a full resolution render target causing the memory to scale linearity with the number of node.

Fixing that would require a complete refactor of the render texture system used in Mixture, while it's not excluded that this work will be done one day, right now nothing is planned.