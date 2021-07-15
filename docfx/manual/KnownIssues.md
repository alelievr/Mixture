# Known Issues

## Some node are empty after an upgrade of package or a new install

Sometimes, it happens that a node ceases to work because of an upgrade of the project or a package. This is likely related to the ShaderGraph embedded in Mixture, for more information you can see this issue: https://github.com/alelievr/Mixture/issues/27

The best way to fix that issue is to reimport the **Mixture** folder under **Packages** in the **Project View**.

## High memory consumption

Mixture processing comes with a high memory cost, especially when using 4k textures in the graph. This is because every node holds a full resolution render target causing the memory to scale linearity with the number of node.

Fixing that would require a complete refactor of the render texture system used in Mixture, while it's not excluded that this work will be done one day, right now nothing is planned.

## Texture Inspector preview is not showing

In some versions of Unity (above 2020.3.4f1 and 2021.1) the mixture package causes the preview of the texture importer to break (throw error in the console).
You can have more details about this issue here: https://issuetracker.unity3d.com/issues/texture-importer-inspector-throws-errors-when-a-built-in-texture-inspector-is-overwritten-in-c-number

The only known workaround is to downgrade to Unity 2020.3.4f1 or remove the CustomEditor for 2D mixture asset (but you loose the mixture UI in the inspector).

## Memory leak after Undo/Redo operation in the editor

When using the Undo/Redo operation while a Mixture graph is open, a part of the RenderTexture and ComputeBuffers allocated for the processing of the Mixture will not be cleanup correctly, causing the memory to grow linearly.

This is caused by the way Unity serialize classes with the [SerializeReference] attribute, hopefully it will be fixed someday. If not, we'll make a new system to avoid having managed memory inside serialized nodes.