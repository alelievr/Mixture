using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using GraphProcessor;
using System;

namespace Mixture
{
	[NodeCustomEditor(typeof(FixedShaderNode))]
	public class FixedShaderNodeView : MixtureNodeView
	{
		FixedShaderNode		fixedShaderNode => nodeTarget as FixedShaderNode;
		int					materialHash = -1;

		ObjectField			debugCustomRenderTextureField;
		ObjectField			debugShaderField;
		ObjectField			debugMaterialField;

		public override void Enable(bool fromInspector)
		{
			base.Enable(fromInspector);

			if (!fromInspector)
			{
				if (fixedShaderNode.material != null && !owner.graph.IsObjectInGraph(fixedShaderNode.material))
				{
					if (owner.graph.IsExternalSubAsset(fixedShaderNode.material))
					{
						fixedShaderNode.material = new Material(fixedShaderNode.material);
						fixedShaderNode.material.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
					}
					if (fixedShaderNode.material.shader.name != ShaderNode.DefaultShaderName)
						owner.graph.AddObjectToGraph(fixedShaderNode.material);
				}

				InitializeDebug();

				onPortDisconnected += ResetMaterialPropertyToDefault;
			}

			if (fixedShaderNode.displayMaterialInspector)
			{
				Action<bool> safeMaterialGUI = (bool init) => {
					// Copy fromInspector to avoid having the same value (lambda capture fromInspector pointer)
					bool f = fromInspector;
					if (!init)
						MaterialGUI(f);
				};
				safeMaterialGUI(true);
				var materialIMGUI = new IMGUIContainer(() => safeMaterialGUI(false));

				materialIMGUI.AddToClassList("MaterialInspector");

				controlsContainer.Add(materialIMGUI);
			}
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

			debugContainer.Add(debugCustomRenderTextureField);
			debugContainer.Add(debugShaderField);
		}

		void MaterialGUI(bool fromInspector)
		{
			if (fixedShaderNode.material == null)
				return;

			if (materialHash != -1 && materialHash != GetMaterialHash(fixedShaderNode.material))
				NotifyNodeChanged();
			materialHash = GetMaterialHash(fixedShaderNode.material);

			// Update the GUI when shader is modified
			if (MaterialPropertiesGUI(fixedShaderNode.material, fromInspector))
			{
				// ForceUpdatePorts might affect the VisualElement hierarchy, thus it can't be called from an ImGUI context
				schedule.Execute(() => {
					ForceUpdatePorts();
				}).ExecuteLater(1);
			}
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