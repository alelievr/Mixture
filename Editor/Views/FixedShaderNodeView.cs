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
	[NodeCustomEditor(typeof(FixedShaderNode))]
	public class FixedShaderNodeView : MixtureNodeView
	{
		VisualElement	    shaderCreationUI;
		VisualElement	    materialEditorUI;
		MaterialEditor	    materialEditor;
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
                var title = new Label("Properties");
                title.style.unityFontStyleAndWeight = FontStyle.Bold;
                title.style.fontSize = 14;
                title.style.marginBottom = 6;

                materialEditorUI = new VisualElement();
                materialEditorUI.style.paddingBottom = 8;
                materialEditorUI.style.paddingLeft = 8;
                materialEditorUI.style.paddingTop = 8;
                materialEditorUI.style.paddingRight = 8;

                materialEditorUI.Add(title);

                var materialIMGUI = new IMGUIContainer(MaterialGUI);
                materialIMGUI.style.marginLeft = 8;

                materialEditorUI.Add(materialIMGUI);
                controlsContainer.Add(materialEditorUI);
                controlsContainer.style.backgroundColor = new StyleColor(new Color(.16f, .16f, .16f));
                controlsContainer.style.borderTopWidth = 1;
                controlsContainer.style.borderColor = new StyleColor(new Color(.12f, .12f, .12f));

                materialEditor = Editor.CreateEditor(fixedShaderNode.material) as MaterialEditor;
                
            }
            style.width = fixedShaderNode.width;
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