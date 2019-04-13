using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using System.IO;
using System;

using UnityEngine.Experimental.Rendering;

[System.Serializable]
public class OutputNode : BaseNode
{
	[Input(name = "In")]
	public Texture		input;

	// TODO
	// [Input(name = "Target size")]
	// public Vector2		targetSize;

	public override string		name => "Output";

	protected override void Process()
	{
		// Currently runtime texture generation is not supported
		#if UNITY_EDITOR
		if (input != null && (input is RenderTexture rt))
		{
			var outPath = "Assets/test.png";
			RenderTexture.active = rt;
			Texture2D o = new Texture2D(rt.width, rt.height, rt.graphicsFormat, TextureCreationFlags.None);
			o.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
			o.Apply();
			var bytes = o.EncodeToPNG();
			outPath = Path.GetFullPath(outPath);
			Debug.Log("OutPath: " + outPath);
			File.WriteAllBytes(outPath, bytes);
			UnityEditor.AssetDatabase.Refresh();
		}
		#endif
	}
}
