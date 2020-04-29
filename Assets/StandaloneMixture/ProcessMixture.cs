using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProcessMixture : MonoBehaviour
{
    public Texture2D source;
    public Texture2D target;

    public RenderTexture    debugRT;

    public RawImage         image;

    void Start()
    {
        var blendGraph = CallBlendingMixture.SetupBlendingMixture(source, target, out debugRT);        
        image.texture = debugRT;

        StartCoroutine(CallBlendingMixture.ExecuteAndExportMixture(blendGraph, "C:\\Users\\Antoine Lelievre\\test.png"));
    }
}
