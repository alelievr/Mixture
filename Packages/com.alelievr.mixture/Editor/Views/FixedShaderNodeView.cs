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
		int					materialCRC;

		ObjectField			debugCustomRenderTextureField;
		ObjectField			debugShaderField;
		ObjectField			debugMaterialField;

		public override void Enable()
		{
			base.Enable();

			if (fixedShaderNode.material != null && !owner.graph.IsObjectInGraph(fixedShaderNode.material))
				owner.graph.AddObjectToGraph(fixedShaderNode.material);

			InitializeDebug();

			if (fixedShaderNode.displayMaterialInspector)
			{
				var materialIMGUI = new IMGUIContainer(MaterialGUI);
				materialIMGUI.AddToClassList("MaterialInspector");

				controlsContainer.Add(materialIMGUI);
				materialEditor = Editor.CreateEditor(fixedShaderNode.material) as MaterialEditor;
			}

			onPortDisconnected += ResetMaterialPropertyToDefault;
		}

		~FixedShaderNodeView() => onPortDisconnected -= ResetMaterialPropertyToDefault;

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

			debugShaderField = new ObjectField("Shader")
			{
				value = fixedShaderNode.shader,
				objectType = typeof(Shader)
			};

			fixedShaderNode.material.hideFlags = HideFlags.HideAndDontSave | HideFlags.HideInInspector | HideFlags.HideInHierarchy;

			debugContainer.Add(debugCustomRenderTextureField);
			debugContainer.Add(debugShaderField);
		}

		void MaterialGUI()
		{
			if (materialCRC != fixedShaderNode.material.ComputeCRC())
			{
				NotifyNodeChanged();
				materialCRC = fixedShaderNode.material.ComputeCRC();
			}

			// Update the GUI when shader is modified
			if (MaterialPropertiesGUI(fixedShaderNode.material))
				ForceUpdatePorts();
		}

		void ResetMaterialPropertyToDefault(PortView pv)
		{
			foreach (var p in fixedShaderNode.ListMaterialProperties(null))
			{
				if (pv.portData.identifier == p.identifier)
					fixedShaderNode.ResetMaterialPropertyToDefault(fixedShaderNode.material, p.identifier);
			}
		}

		public override void OnRemoved()
		{
			if (fixedShaderNode.material != null)
				owner.graph.RemoveObjectFromGraph(fixedShaderNode.material);
		}
    }
}