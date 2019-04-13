using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using GraphProcessor;

[NodeCustomEditor(typeof(TextureNode))]
public class TextureNodeView : BaseNodeView
{
	VisualElement	shaderCreationUI;
	TextureNode		textureNode;

	public override void Enable()
	{
		textureNode = nodeTarget as TextureNode;

		ObjectField textureField = new ObjectField
		{
			value = textureNode.shader,
			objectType = typeof(Shader),
		};

		shaderCreationUI = new VisualElement();
		contentContainer.Add(shaderCreationUI);

		textureField.RegisterValueChangedCallback((v) => {
			owner.RegisterCompleteObjectUndo("Updated Shader of Texture node");
			textureNode.shader = (Shader)v.newValue;
			UpdateShaderCreationUI();
		});

		controlsContainer.Add(textureField);
	}

	void UpdateShaderCreationUI()
	{
		shaderCreationUI.Clear();
		
		if (textureNode.shader == null)
		{
			shaderCreationUI.Add(new Button(CreateEmbeededShader) {
				text = "New Shader"
			});
		}

		void CreateEmbeededShader()
		{
			Debug.Log("TODO");
		}
	}
}