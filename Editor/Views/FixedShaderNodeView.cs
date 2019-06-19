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
	[NodeCustomEditor(typeof(TextureNode))]
	public class FixedShaderNodeView : MixtureNodeView
	{
		VisualElement	shaderCreationUI;
		VisualElement	materialEditorUI;
		MaterialEditor	materialEditor;
		FixedShaderNode		fixedShaderNode;

		public override void OnCreated()
		{
			if (fixedShaderNode.material != null)
			{
				owner.graph.AddObjectToGraph(fixedShaderNode.material);
			}
		}

		public override void Enable()
		{
			fixedShaderNode = nodeTarget as FixedShaderNode;

            if(fixedShaderNode.displayMaterialInspector)
            {
                materialEditorUI = new VisualElement();
                materialEditorUI.Add(new IMGUIContainer(MaterialGUI));
                controlsContainer.Add(materialEditorUI);

                materialEditor = Editor.CreateEditor(fixedShaderNode.material) as MaterialEditor;
            }
        }

		void MaterialGUI()
		{
			// Update the GUI when shader is modified
			if (MaterialPropertiesGUI(fixedShaderNode.material))
			{
                ForceUpdatePorts();
			}
		}

		public override void OnRemoved()
		{
			Debug.Log("Material shader node: " + fixedShaderNode.material);
			owner.graph.RemoveObjectFromGraph(fixedShaderNode.material);
		}
	}
}