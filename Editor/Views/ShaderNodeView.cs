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

		MaterialProperty[]	oldProperties = null;

		public override void OnCreated()
		{
			if (shaderNode.material != null)
			{
				owner.graph.AddObjectToGraph(shaderNode.material);
			}
		}

		public override void Enable()
		{
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

			controlsContainer.Add(shaderField);

			shaderCreationUI = new VisualElement();
			controlsContainer.Add(shaderCreationUI);
			UpdateShaderCreationUI();

			materialEditorUI = new VisualElement();
			materialEditorUI.Add(new IMGUIContainer(MaterialGUI));
			controlsContainer.Add(materialEditorUI);

			materialEditor = Editor.CreateEditor(shaderNode.material) as MaterialEditor;
		}

		void UpdateShaderCreationUI()
		{
			shaderCreationUI.Clear();

			if (shaderNode.shader?.name == ShaderNode.DefaultShaderName)
			{
				shaderCreationUI.Add(new Button(CreateEmbeededShader) {
					text = "New Shader"
				});
			}
			else
			{
				shaderCreationUI.Add(new Button(OpenCurrentShader){
					text = "Open"
				});
			}

			void CreateEmbeededShader()
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
			// Custom property draw, we don't want things that are connected to an edge or useless like the render queue
			MaterialPropertiesGUI(MaterialEditor.GetMaterialProperties(new []{shaderNode.material}));
		}

		void CheckPropertyChanged(MaterialProperty[] properties)
		{
			bool propertyChanged = false;
			if (oldProperties != null)
			{
				// Check if shader was changed (new/deleted properties)
				if (properties.Length != oldProperties.Length)
				{
					propertyChanged = true;
				}
				else
				{
					for (int i = 0; i < properties.Length; i++)
					{
						if (properties[i].type != oldProperties[i].type)
							propertyChanged = true;
						if (properties[i].displayName != oldProperties[i].displayName)
							propertyChanged = true;
						if (properties[i].flags != oldProperties[i].flags)
							propertyChanged = true;
						if (properties[i].name != oldProperties[i].name)
							propertyChanged = true;
					}
				}
			}

			// Update the GUI when shader is modified
			if (propertyChanged)
			{
				UpdateShaderCreationUI();
				// We fore the update of node ports
				ForceUpdatePorts();
			}

			oldProperties = properties;
		}

		void MaterialPropertiesGUI(MaterialProperty[] properties)
		{
			var portViews = GetPortViewsFromFieldName(nameof(ShaderNode.materialInputs));

			CheckPropertyChanged(properties);

			foreach (var property in properties)
			{
				if ((property.flags & (MaterialProperty.PropFlags.HideInInspector | MaterialProperty.PropFlags.PerRendererData)) != 0)
					continue;

				// Retrieve the port view from the property name
				var portView = portViews.FirstOrDefault(p => p.portData.identifier == property.name);
				if (portView == null || portView.connected)
					continue;

				float h = materialEditor.GetPropertyHeight(property, property.displayName);
				Rect r = EditorGUILayout.GetControlRect(true, h, EditorStyles.layerMaskField);

				materialEditor.ShaderProperty(r, property, property.displayName);
			}
		}

		public override void OnRemoved()
		{
			Debug.Log("Material shader node: " + shaderNode.material);
			owner.graph.RemoveObjectFromGraph(shaderNode.material);
		}
	}
}