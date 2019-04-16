using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;
using GraphProcessor;
using System.Linq;

[System.Serializable, NodeMenuItem("Texture Shader")]
public class TextureNode : BaseNode
{
	[Input(name = "In"), SerializeField]
	public Texture			input;

	[Output(name = "Out"), SerializeField]
	public RenderTexture	output;

	public Shader			shader;
	public override string		name => "Texture";
	public Material			material;

	public static string	DefaultShaderName = "TextureNodeDefaultBlack";

	protected override void Enable()
	{
		if (shader == null)
		{
			shader = Resources.Load<Shader>(DefaultShaderName);
		}

		// For now, we allocate one render target per node
		output = new RenderTexture(512, 512, 0, GraphicsFormat.R8G8B8A8_UNorm);

		if (material == null)
			material = new Material(shader);
	}

	protected override void Process()
	{
		if (material == null)
		{
			Debug.LogError("Can't process TextureNode, missing shader");
			return ;
		}

		Graphics.Blit(input, output, material, 0);
	}
}
