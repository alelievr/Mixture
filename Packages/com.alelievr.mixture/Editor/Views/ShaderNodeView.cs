using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using GraphProcessor;
using System.Collections.Generic;
using System.IO;
using System;

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

		int				materialHash;
		DateTime		lastModified;
		string			shaderPath;

		protected override string header => "Shader Properties";

		public override void Enable(bool fromInspector)
		{
			base.Enable(fromInspector);

			if (shaderNode.material != null && !owner.graph.IsObjectInGraph(shaderNode.material))
				owner.graph.AddObjectToGraph(shaderNode.material);

			shaderField = new ObjectField
			{
				value = shaderNode.shader,
				objectType = typeof(Shader),
			};

			shaderField.RegisterValueChangedCallback((v) => {
				SetShader((Shader)v.newValue);
			});

			if (shaderNode.shader != null)
				shaderPath = AssetDatabase.GetAssetPath(shaderNode.shader);
			if (!String.IsNullOrEmpty(shaderPath))
				lastModified = File.GetLastWriteTime(shaderPath);
			var lastWriteDetector = schedule.Execute(DetectShaderChanges);
			lastWriteDetector.Every(200);
			
			InitializeDebug();

			controlsContainer.Add(shaderField);

			shaderCreationUI = new VisualElement();
			controlsContainer.Add(shaderCreationUI);
			UpdateShaderCreationUI();

			controlsContainer.Add(new IMGUIContainer(MaterialGUI));
			materialEditor = Editor.CreateEditor(shaderNode.material) as MaterialEditor;

			onPortDisconnected += ResetMaterialPropertyToDefault;
		}

		~ShaderNodeView()
		{
			onPortDisconnected -= ResetMaterialPropertyToDefault;
		}

		void DetectShaderChanges()
		{
			if (shaderNode.shader == null)
				return;

			if (shaderPath == null)
				shaderPath = AssetDatabase.GetAssetPath(shaderNode.shader);
			
			var modificationDate = File.GetLastWriteTime(shaderPath);

			if (lastModified != modificationDate)
			{
				schedule.Execute(() => {
					// Reimport the compute shader:
					AssetDatabase.ImportAsset(shaderPath);

					NotifyNodeChanged();

					shaderNode.IsShaderValid();
				}).ExecuteLater(100);
			}
			lastModified = modificationDate;
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
				// TODO: create a popupwindow instead of a context menu
				var menu = new GenericMenu();
				var dim = (OutputDimension)shaderNode.rtSettings.GetTextureDimension(owner.graph);

#if MIXTURE_SHADERGRAPH
				GUIContent shaderGraphContent = EditorGUIUtility.TrTextContentWithIcon("Graph", Resources.Load<Texture2D>("sg_graph_icon@64"));
				menu.AddItem(shaderGraphContent, false, () => SetShader(MixtureEditorUtils.CreateNewShaderGraph(owner.graph, title, dim)));
#endif
				GUIContent shaderTextContent = EditorGUIUtility.TrTextContentWithIcon("Text", "Shader Icon");
				menu.AddItem(shaderTextContent, false, () => SetShader(MixtureEditorUtils.CreateNewShaderText(owner.graph, title, dim)));
				menu.ShowAsContext();
			}

			void OpenCurrentShader()
			{
				AssetDatabase.OpenAsset(shaderNode.shader);
			}
		}
		
		void ResetMaterialPropertyToDefault(PortView pv)
		{
			foreach (var p in shaderNode.ListMaterialProperties(null))
			{
				if (pv.portData.identifier == p.identifier)
					shaderNode.ResetMaterialPropertyToDefault(shaderNode.material, p.identifier);
			}
		}

		void SetShader(Shader newShader)
		{
			owner.RegisterCompleteObjectUndo("Updated Shader of ShaderNode");
			shaderNode.shader = newShader;
			shaderField.value = newShader;
			shaderNode.material.shader = newShader;
			UpdateShaderCreationUI();

			title = newShader?.name ?? "New Shader";

			// We fore the update of node ports
			ForceUpdatePorts();

			shaderNode.IsShaderValid();
		}

		void MaterialGUI()
		{
			if (GetMaterialHash(shaderNode.material) != materialHash)
			{
				NotifyNodeChanged();
				materialHash = GetMaterialHash(shaderNode.material);
			}

			// Update the GUI when shader is modified
			if (MaterialPropertiesGUI(shaderNode.material))
			{
				schedule.Execute(() => UpdateShaderCreationUI());
				// We fore the update of node ports
				ForceUpdatePorts();
			}
		}

		public override void OnRemoved() => owner.graph.RemoveObjectFromGraph(shaderNode.material);
	}
}