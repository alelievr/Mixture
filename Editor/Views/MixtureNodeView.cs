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
	public class MixtureNodeView : BaseNodeView
	{
        protected new MixtureGraphView  owner => base.owner as MixtureGraphView;
        protected new MixtureNode       nodeTarget => base.nodeTarget as MixtureNode;

        // TODO: function for this:
		// void UpdateShaderCreationUI()
		// {
		// 	shaderCreationUI.Clear();

		// 	if (textureNode.shader?.name == ShaderNode.DefaultShaderName)
		// 	{
		// 		shaderCreationUI.Add(new Button(CreateEmbeededShader) {
		// 			text = "New Shader"
		// 		});
		// 	}
		// 	else
		// 	{
		// 		shaderCreationUI.Add(new Button(OpenCurrentShader){
		// 			text = "Open"
		// 		});
		// 	}

		// 	void CreateEmbeededShader()
		// 	{
		// 		Debug.Log("TODO");
		// 	}

		// 	void OpenCurrentShader()
		// 	{
		// 		AssetDatabase.OpenAsset(textureNode.shader);
		// 	}
		// }

		// protected void DrawMaterialGUI(Material material)
		// {
		// 	// Custom property draw, we don't want things that are connected to an edge or useless like the render queue
		// 	MaterialPropertiesGUI(MaterialEditor.GetMaterialProperties(new []{material}));
		// }

		// void CheckPropertyChanged(MaterialProperty[] properties)
		// {
		// 	bool propertyChanged = false;
		// 	if (oldProperties != null)
		// 	{
		// 		// Check if shader was changed (new/deleted properties)
		// 		if (properties.Length != oldProperties.Length)
		// 		{
		// 			propertyChanged = true;
		// 		}
		// 		else
		// 		{
		// 			for (int i = 0; i < properties.Length; i++)
		// 			{
		// 				if (properties[i].type != oldProperties[i].type)
		// 					propertyChanged = true;
		// 				if (properties[i].displayName != oldProperties[i].displayName)
		// 					propertyChanged = true;
		// 				if (properties[i].flags != oldProperties[i].flags)
		// 					propertyChanged = true;
		// 				if (properties[i].name != oldProperties[i].name)
		// 					propertyChanged = true;
		// 			}
		// 		}
		// 	}

		// 	// Update the GUI when shader is modified
		// 	if (propertyChanged)
		// 	{
		// 		UpdateShaderCreationUI();
		// 		// We fore the update of node ports
		// 		ForceUpdatePorts();
		// 	}

		// 	oldProperties = properties;
		// }

		// void MaterialPropertiesGUI(MaterialProperty[] properties)
		// {
		// 	var portViews = GetPortViewsFromFieldName(nameof(ShaderNode.materialInputs));

		// 	CheckPropertyChanged(properties);

		// 	foreach (var property in properties)
		// 	{
		// 		if ((property.flags & (MaterialProperty.PropFlags.HideInInspector | MaterialProperty.PropFlags.PerRendererData)) != 0)
		// 			continue;

		// 		// Retrieve the port view from the property name
		// 		var portView = portViews.FirstOrDefault(p => p.portData.identifier == property.name);
		// 		if (portView == null || portView.connected)
		// 			continue;

		// 		float h = materialEditor.GetPropertyHeight(property, property.displayName);
		// 		Rect r = EditorGUILayout.GetControlRect(true, h, EditorStyles.layerMaskField);

		// 		materialEditor.ShaderProperty(r, property, property.displayName);
		// 	}
		// }

		// public override void OnRemoved()
		// {
		// 	graphv(textureNode.material);
		// }
	}
}