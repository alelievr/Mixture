using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using Mixture;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;
using System.Linq;

public static class CallBlendingMixture
{
    static Stack<IEnumerator> callbacks = new Stack<IEnumerator>();

    public static void Process()
    {
        var args = Environment.GetCommandLineArgs();

        if (args.Length < 2)
        {
            Debug.LogError("Not enough args !");
            return;
        }

        string sourcePath = args[0];
        string targetPath = args[1];
        
        Texture2D source = new Texture2D(1, 1);
        Texture2D target = new Texture2D(1, 1);
        ImageConversion.LoadImage(source, File.ReadAllBytes(sourcePath));
        ImageConversion.LoadImage(target, File.ReadAllBytes(targetPath));

        // TODO
        // BlendMixture(source, target, out var _);
    }

    public static IEnumerator ExecuteAndExportMixture(MixtureGraph graph, string outputPath)
    {
        // Wait that the CRT is loaded
        yield return new WaitForEndOfFrame();

        // Alloc destination texture
        var settings = graph.outputNode.rtSettings;
        Texture2D destination = new Texture2D(
            settings.GetWidth(graph),
            settings.GetHeight(graph),
            settings.GetGraphicsFormat(graph),
            TextureCreationFlags.None
        );

        // Process the graph andreadback the result
        MixtureGraphProcessor processor = new MixtureGraphProcessor(graph);
        processor.Run();
        var shadernodes = graph.nodes.Where(n => n is IUseCustomRenderTextureProcessing).Select(n => n as IUseCustomRenderTextureProcessing).ToList();
        graph.ReadbackMainTexture(destination);

        // Output the image to a file
        var bytes = ImageConversion.EncodeToPNG(destination);
        File.WriteAllBytes(outputPath, bytes);
    }

    public static MixtureGraph SetupBlendingMixture(Texture2D source, Texture2D target, out RenderTexture debugRT)
    {
        // Setup blend graph:
        var standaloneMixtures = Resources.Load<StandaloneMixtures>("EmbeddedMixtures");
        var blendGraph = standaloneMixtures.embeddedMixtures[0];
        blendGraph.SetParameterValue("Source", source);
        blendGraph.SetParameterValue("Target", target);
        // Disable compression because ImageConversion.EncodeTo doesn't support it
        blendGraph.outputNode.enableCompression = false;

        // Debug
        debugRT = blendGraph.outputNode.tempRenderTexture;

        // Initialize completely the graph
        // TODO: move this to the graph initialization (we'll embedde the processor in the graph)
        new MixtureGraphProcessor(blendGraph).Run();

        return blendGraph;
    }
}
