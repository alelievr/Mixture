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
    }

    public static void BlendMixture(Texture2D source, Texture2D target, out RenderTexture debugRT)
    {
        // Setup blend graph:
        var standaloneMixtures = Resources.Load<StandaloneMixtures>("EmbeddedMixtures");
        var blendGraph = standaloneMixtures.embeddedMixtures[0];
        blendGraph.SetParameterValue("Source", source);
        blendGraph.SetParameterValue("Target", target);
        // Disable compression because ImageConversion.EncodeTo doesn't support it
        blendGraph.outputNode.enableCompression = false;

        // Alloc destination texture
        var settings = blendGraph.outputNode.rtSettings;
        Texture2D destination = new Texture2D(
            settings.GetWidth(blendGraph),
            settings.GetHeight(blendGraph),
            settings.GetGraphicsFormat(blendGraph),
            TextureCreationFlags.None
        );

        // Process the graph andreadback the result
        MixtureGraphProcessor processor = new MixtureGraphProcessor(blendGraph);
        processor.Run();
        var shadernodes = blendGraph.nodes.Where(n => n is IUseCustomRenderTextureProcessing).Select(n => n as IUseCustomRenderTextureProcessing).ToList();
        debugRT = blendGraph.outputNode.tempRenderTexture;
        blendGraph.ReadbackMainTexture(destination);

        // Output the image to a file
        var bytes = ImageConversion.EncodeToPNG(destination);
        File.WriteAllBytes("C:\\Users\\Antoine Lelievre\\test.png", bytes);
    }
}
