using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UI;
using Mixture;
using System.IO;

public class ProcessMixtureWithVariant : MonoBehaviour
{
    public Texture graphTexture;

    public Texture2D source;
    public Texture2D target;

    public RawImage         image;

    public string   outputPath;

    void Start()
    {
        var graph = MixtureDatabase.GetGraphFromTexture(graphTexture);

        var variant = ScriptableObject.CreateInstance<MixtureVariant>();
        variant.SetParent(graph);

        variant.SetParameterValue("Source", source);
        variant.SetParameterValue("Target", target);

        variant.ProcessGraphWithOverrides();

        // Create the destination texture
        var settings = graph.outputNode.settings;
        Texture2D destination = new Texture2D(
            settings.GetWidth(graph),
            settings.GetHeight(graph),
            settings.GetGraphicsFormat(graph),
            TextureCreationFlags.None
        );

        // Readback the result
        graph.ReadbackMainTexture(destination);

        // Write the file at the target destination
        var bytes = ImageConversion.EncodeToPNG(destination);
        File.WriteAllBytes(outputPath, bytes);
    }
}
