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
		MaterialEditor	    materialEditor;
		FixedShaderNode		fixedShaderNode => nodeTarget as FixedShaderNode;

		ObjectField			debugCustomRenderTextureField;

		public override void OnCreated()
		{
			if (fixedShaderNode.material != null)
			{
				owner.graph.AddObjectToGraph(fixedShaderNode.material);
			}
		}

		public override void Enable()
		{
			base.Enable();

			InitializeDebug();

			if (fixedShaderNode.displayMaterialInspector)
			{
				var materialIMGUI = new IMGUIContainer(MaterialGUI);
				materialIMGUI.AddToClassList("MaterialInspector");

				propertyEditorUI.Add(materialIMGUI);
				materialEditor = Editor.CreateEditor(fixedShaderNode.material) as MaterialEditor;
			}
		}

		void InitializeDebug()
		{
			fixedShaderNode.onProcessed += () => {
				debugCustomRenderTextureField.value = fixedShaderNode.output;
			};

			debugCustomRenderTextureField = new ObjectField("CRT")
			{
				value = fixedShaderNode.output,
				objectType = typeof(CustomRenderTexture)
			};
			
			debugContainer.Add(debugCustomRenderTextureField);
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
			owner.graph.RemoveObjectFromGraph(fixedShaderNode.material);
		}
	}
}