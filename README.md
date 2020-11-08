![](Packages/com.alelievr.mixture/Documentation~/Images/Mixture-github.png)

Mixture is a powerful node-based tool crafted in unity to generate all kinds of textures in realtime. Mixture is very flexible, easily customizable through [ShaderGraph](https://unity.com/shader-graph) and a simple C# API, fast with it's GPU based workflow and compatible with all the render pipelines thanks to the new [Custom Render Texture](https://docs.unity3d.com/2020.2/Documentation/ScriptReference/CustomRenderTextureManager.html) API.

![](Packages/com.alelievr.mixture/Documentation~/Images/2020-11-04-01-04-59.png)

# Getting Started

## Installation

You need at least a Unity 2020.2 beta to be able to use Mixture.

TODO: open upm instructions

## Documentation

You can consult the getting started guide here: https://alelievr.github.io/Mixture/manual/GettingStarted.html

As well as the Node Library here: https://alelievr.github.io/Mixture/manual/nodes/NodeLibraryIndex.html

And finally, you can find some Mixture examples here: https://alelievr.github.io/Mixture/manual/Examples.html

# Bugs / Feature Requests

Bugs and features requests are logged using the github issue system. To report a bug, request a feature, or simply ask a question, simply [open a new issue](https://github.com/alelievr/Mixture/issues/new/choose).

# How to contribute 

Your contributions are much appreciated even small ones, we'll review them and eventually merge them.

If you want to add a new node, you can check out [this documentation page on how to create a new shader-based node](https://alelievr.github.io/Mixture/manual/ShaderNodes.html). Once you have it working, you can prepare your pull request.
In case you have any questions about a feature you want to develop of something you're not sure how to do, you can still create a draft pull request to discuss the implementation details.

# Gallery / Cool things

You can open a Mixture graph just by double clicking any texture field in the inspector with a Mixture assigned to it.
![](docs/docfx/images/MixtureOpen.gif)

[Surface Gradient](https://blogs.unity3d.com/2019/11/20/normal-map-compositing-using-the-surface-gradient-framework-in-shader-graph/) powered normal map operations.
![](docs/docfx/images/NormalBlend.gif)

Extract buffers (depth, normal, color or position) from the rendering of a prefab and use it directly in the graph (HDRP Only).
![](docs/docfx/images/SceneCapture.gif)
