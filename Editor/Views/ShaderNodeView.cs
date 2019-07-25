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
		ShaderNode		shaderNode => nodeTarget as ShaderNode;

		ObjectField		debugCustomRenderTextureField;
		ObjectField		shaderField;

		protected override string header => "Shader Properties";

		protected override bool hasPreview => false;

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

			shaderField = new ObjectField
			{
				value = shaderNode.shader,
				objectType = typeof(Shader),
			};

			shaderField.RegisterValueChangedCallback((v) => {
				SetShader((Shader)v.newValue);
			});
			
			InitializeDebug();

			propertyEditorUI.Add(shaderField);

			shaderCreationUI = new VisualElement();
			propertyEditorUI.Add(shaderCreationUI);
			UpdateShaderCreationUI();

			propertyEditorUI.Add(new IMGUIContainer(MaterialGUI));
			materialEditor = Editor.CreateEditor(shaderNode.material) as MaterialEditor;
		}

		void InitializeDebug()
		{
			shaderNode.onProcessed += () => {
				debugCustomRenderTextureField.value = shaderNode.output;
			};

			debugCustomRenderTextureField = new ObjectField("Output")
			{
				value = shaderNode.output
			};
			
			debugContainer.Add(debugCustomRenderTextureField);
		}

		void UpdateShaderCreationUI()
		{
			shaderCreationUI.Clear();

			if (shaderNode.shader == null)
			{
				shaderCreationUI.Add(new Button(CreateNewShader) {
					text = "New Shader"
				});
			}
			else
			{
				shaderCreationUI.Add(new Button(OpenCurrentShader){
					text = "Open"
				});
			}

			void CreateNewShader()
			{
				GUIContent shaderGraphContent = EditorGUIUtility.TrTextContentWithIcon("Graph", Resources.Load<Texture2D>("sg_graph_icon@64"));
				GUIContent shaderTextContent = EditorGUIUtility.TrTextContentWithIcon("Text", "Shader Icon");

				// TODO: create a popupwindow instead of a context menu

				var menu = new GenericMenu();
				var dim = (OutputDimension)shaderNode.rtSettings.GetTextureDimension(owner.graph);

				menu.AddItem(shaderGraphContent, false, () => SetShader(MixtureEditorUtils.CreateNewShaderGraph(title, dim)));
				menu.AddItem(shaderTextContent, false, () => SetShader(MixtureEditorUtils.CreateNewShaderText(title, dim)));
				menu.ShowAsContext();
			}

			void OpenCurrentShader()
			{
				AssetDatabase.OpenAsset(shaderNode.shader);
			}
		}

		void SetShader(Shader newShader)
		{
			owner.RegisterCompleteObjectUndo("Updated Shader of ShaderNode");
			shaderNode.shader = newShader;
			shaderField.value = newShader;
			shaderNode.material.shader = newShader;
			UpdateShaderCreationUI();

			// We fore the update of node ports
			ForceUpdatePorts();
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
			owner.graph.RemoveObjectFromGraph(shaderNode.material);
		}
	}
}