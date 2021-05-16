using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UI;
using Mixture;
using System.IO;

public class ProcessMixture : MonoBehaviour
{
    public Texture graphTexture;

    public Texture2D source;
    public Texture2D target;

    public RenderTexture    debugRT;

    public RawImage         image;

    public string outputPath;

    void Start()
    {
        var graph = MixtureDatabase.GetGraphFromTexture(graphTexture);

        graph.SetParameterValue("Source", source);
        graph.SetParameterValue("Target", target);

        // make sure compression is not enabled for the readback
        graph.outputNode.mainOutput.enableCompression = false;

        // Create the destination texture
        var settings = graph.outputNode.settings;
        Texture2D destination = new Texture2D(
            settings.GetWidth(graph),
            settings.GetHeight(graph),
            settings.GetGraphicsFormat(graph),
            TextureCreationFlags.None
        );

        // Process the graph
        MixtureGraphProcessor processor = new MixtureGraphProcessor(graph);
        processor.Run();

        // Readback the result
        graph.ReadbackMainTexture(destination);

        // TODO: debug
        // image.texture = debugRT;

        // Write the file at the target destination
        var bytes = ImageConversion.EncodeToPNG(destination);
        File.WriteAllBytes(outputPath, bytes);

        // Reset graph parameters to avoid serialization issues:
        graph.SetParameterValue("Source", null);
        graph.SetParameterValue("Target", null);
    }
}
