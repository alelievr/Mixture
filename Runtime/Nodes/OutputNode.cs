using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using System.IO;
using System;
using UnityEngine.Rendering;

using UnityEngine.Experimental.Rendering;

[System.Serializable]
public class OutputNode : BaseNode
{
	[Input(name = "In")]
	public Texture			input;

	// TODO
	// [Input(name = "Target size")]
	// public Vector2		targetSize;

	[HideInInspector, SerializeField]
	public Vector2Int		targetSize = new Vector2Int(512, 512);
	[HideInInspector, SerializeField]
	public RenderTexture	outputTexture;
	[HideInInspector, SerializeField]
	public GraphicsFormat	format = GraphicsFormat.R8G8B8A8_UNorm;

	public override string		name => "Output";

	protected override void Enable()
	{
		// Create a dummy render texture that will be resized later on
		if (outputTexture == null)
			outputTexture = new RenderTexture(1, 1, 0, format);
	}

	protected override void Process()
	{
		// Update the renderTexture size and format:
		if (outputTexture.width != targetSize.x || outputTexture.height != targetSize.y || outputTexture.graphicsFormat != format)
		{
			outputTexture.Release();
			outputTexture.width = targetSize.x;
			outputTexture.height = targetSize.y;
			outputTexture.graphicsFormat = format;
			outputTexture.Create();
		}

		Graphics.Blit(input, outputTexture);
	}
}
