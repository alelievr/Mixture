using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;
using GraphProcessor;
using System.Linq;
using UnityEditor;
using System;

[System.Serializable, NodeMenuItem("Texture Shader")]
public class TextureNode : BaseNode
{
	[Input(name = "In"), SerializeField]
	public List< object >	materialInputs;

	[Output(name = "Out"), SerializeField]
	public RenderTexture	output;

	public Shader			shader;
	public override string	name => "Texture";
	public Material			material;

	public static string	DefaultShaderName = "TextureNodeDefault";

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

	Type GetPropertyType(MaterialProperty.PropType type)
	{
		switch (type)
		{
			case MaterialProperty.PropType.Color:
				return typeof(Color);
			case MaterialProperty.PropType.Float:
			case MaterialProperty.PropType.Range:
				return typeof(float);
			case MaterialProperty.PropType.Texture:
				return typeof(Texture);
			default:
			case MaterialProperty.PropType.Vector:
				return typeof(Vector4);
		}
	}

	[CustomPortBehavior(nameof(materialInputs))]
	IEnumerable< PortData > ListMaterialProperties(List< SerializableEdge > edges)
	{
		foreach (var prop in MaterialEditor.GetMaterialProperties(new []{material}))
		{
			if (prop.flags == MaterialProperty.PropFlags.HideInInspector
				|| prop.flags == MaterialProperty.PropFlags.NonModifiableTextureData
				|| prop.flags == MaterialProperty.PropFlags.PerRendererData)
				continue;

			yield return new PortData{
				identifier = prop.name,
				displayName = prop.displayName,
				displayType = GetPropertyType(prop.type),
			};
		}
	}

	[CustomPortInput(nameof(materialInputs), typeof(object))]
	public void GetMaterialInputs(List< SerializableEdge > edges)
	{
		// Update material settings when processing the graph:
		foreach (var edge in edges)
		{
			var prop = MaterialEditor.GetMaterialProperty(new []{material}, edge.inputPort.portData.identifier);

			switch (prop.type)
			{
				case MaterialProperty.PropType.Color:
					prop.colorValue = (Color)edge.passThroughBuffer;
					break;
				case MaterialProperty.PropType.Texture:
					// TODO: texture scale and offset
					prop.textureValue = (Texture)edge.passThroughBuffer;
					break;
				case MaterialProperty.PropType.Float:
				case MaterialProperty.PropType.Range:
					prop.floatValue = (float)edge.passThroughBuffer;
					break;
				case MaterialProperty.PropType.Vector:
					prop.vectorValue = (Vector4)edge.passThroughBuffer;
					break;
			}
		}
	}

	protected override void Process()
	{
		if (material == null)
		{
			Debug.LogError("Can't process TextureNode, missing shader");
			return ;
		}

		Graphics.Blit(Texture2D.whiteTexture, output, material, 0);
	}
}
