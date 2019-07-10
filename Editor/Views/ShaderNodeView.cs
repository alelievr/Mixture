using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
	[NodeCustomEditor(typeof(ShaderNode))]
	public class ShaderNodeView : MixtureNodeView
	{
		VisualElement	shaderCreationUI;
		VisualElement	materialEditorUI;
		MaterialEditor	materialEditor;
		ShaderNode		shaderNode;

		protected override string header => "Shader Properties";

		protected override bool hasPreview => true;

		public override void OnCreated()
		{
			if (shaderNode.material != null)
			{
				owner.graph.AddObjectToGraph(shaderNode.material);
			}
		}

		public override void Enable()
		{
			base.Enable();

			shaderNode = nodeTarget as ShaderNode;

			ObjectField shaderField = new ObjectField
			{
				value = shaderNode.shader,
				objectType = typeof(Shader),
			};

			shaderField.RegisterValueChangedCallback((v) => {
				owner.RegisterCompleteObjectUndo("Updated Shader of ShaderNode");
				shaderNode.shader = (Shader)v.newValue;
				shaderNode.material.shader = shaderNode.shader;
				UpdateShaderCreationUI();

				// We fore the update of node ports
				ForceUpdatePorts();
			});

			propertyEditorUI.Add(shaderField);

			shaderCreationUI = new VisualElement();
			propertyEditorUI.Add(shaderCreationUI);
			UpdateShaderCreationUI();

			propertyEditorUI.Add(new IMGUIContainer(MaterialGUI));
			materialEditor = Editor.CreateEditor(shaderNode.material) as MaterialEditor;
		}

		void UpdateShaderCreationUI()
		{
			shaderCreationUI.Clear();

			if (shaderNode?.shader?.name == ShaderNode.DefaultShaderName)
			{
				shaderCreationUI.Add(new Button(CreateEmbeddedShader) {
					text = "New Shader"
				});
			}
			else
			{
				shaderCreationUI.Add(new Button(OpenCurrentShader){
					text = "Open"
				});
			}

			void CreateEmbeddedShader()
			{
				Debug.Log("TODO");
			}

			void OpenCurrentShader()
			{
				AssetDatabase.OpenAsset(shaderNode.shader);
			}
		}

		void MaterialGUI()
		{
			// Update the GUI when shader is modified
			if (MaterialPropertiesGUI(shaderNode.material))
			{
				UpdateShaderCreationUI();
				// We fore the update of node ports
				ForceUpdatePorts();
			}
		}

		public override void OnRemoved()
		{
			Debug.Log("Material shader node: " + shaderNode.material);
			owner.graph.RemoveObjectFromGraph(shaderNode.material);
		}
	}
}