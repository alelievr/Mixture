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
		VisualElement	materialEditorUI;
		ShaderNode		shaderNode => nodeTarget as ShaderNode;

		ObjectField		debugCustomRenderTextureField;

		int				materialHash;
		DateTime		lastModified;
		string			shaderPath;

		protected VisualElement shaderSettings;

		protected override string header => "Shader Properties";

		public override void Enable(bool fromInspector)
		{
			base.Enable(fromInspector);

			if (shaderNode.material != null && !owner.graph.IsObjectInGraph(shaderNode.material))
			{
				// Check if the material we have is ours
				if (owner.graph.IsExternalSubAsset(shaderNode.material))
				{
					shaderNode.material = new Material(shaderNode.material);
					shaderNode.material.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
				}
				owner.graph.AddObjectToGraph(shaderNode.material);
			}

			shaderSettings = new VisualElement();
			var shaderField = AddControlField(nameof(ShaderNode.shader)) as ObjectField;
			shaderSettings.Add(shaderField);

			var shaderCreationUI = new VisualElement();
			shaderSettings.Add(shaderCreationUI);
			UpdateShaderCreationUI(shaderCreationUI, shaderField);

			shaderField.RegisterValueChangedCallback((v) => {
				SetShader((Shader)v.newValue, shaderCreationUI, shaderField);
			});

			if (!fromInspector)
			{
				if (shaderNode.shader != null)
					shaderPath = AssetDatabase.GetAssetPath(shaderNode.shader);
				if (!String.IsNullOrEmpty(shaderPath))
					lastModified = File.GetLastWriteTime(shaderPath);

				var lastWriteDetector = schedule.Execute(DetectShaderChanges);
				lastWriteDetector.Every(200);
				InitializeDebug();

				onPortDisconnected += ResetMaterialPropertyToDefault;
			}

			var materialIMGUI = new IMGUIContainer(() => MaterialGUI(fromInspector, shaderCreationUI, shaderField));
			shaderSettings.Add(materialIMGUI);
			materialIMGUI.AddToClassList("MaterialInspector");

			controlsContainer.Add(shaderSettings);
			MixtureEditorUtils.ScheduleAutoHide(materialIMGUI, owner);
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
					// Reimport the shader:
					AssetDatabase.ImportAsset(shaderPath);

					shaderNode.ValidateShader();

					ForceUpdatePorts();
					NotifyNodeChanged();

					if (shaderNode.shader?.name != null)
						title = shaderNode.shader.name;
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

		void UpdateShaderCreationUI(VisualElement shaderCreationUI, ObjectField shaderField)
		{
			shaderCreationUI.Clear();

			if (shaderNode.shader == null)
			{
				shaderCreationUI.Add(new Button(() => CreateNewShader(shaderCreationUI, shaderField)) {
					text = "New Shader"
				});
			}
			else
			{
				shaderCreationUI.Add(new Button(OpenCurrentShader){
					text = "Open"
				});
			}

			void OpenCurrentShader()
			{
				AssetDatabase.OpenAsset(shaderNode.shader);
			}
		}

		protected virtual void CreateNewShader(VisualElement shaderCreationUI, ObjectField shaderField)
		{
			// TODO: create a popupwindow instead of a context menu
			var menu = new GenericMenu();
			var dim = (OutputDimension)shaderNode.rtSettings.GetTextureDimension(owner.graph);

#if MIXTURE_SHADERGRAPH
			GUIContent shaderGraphContent = EditorGUIUtility.TrTextContentWithIcon("Graph", Resources.Load<Texture2D>("sg_graph_icon@64"));
			menu.AddItem(shaderGraphContent, false, () => SetShader(MixtureEditorUtils.CreateNewShaderGraph(owner.graph, title, dim), shaderCreationUI, shaderField));
#endif
			GUIContent shaderTextContent = EditorGUIUtility.TrTextContentWithIcon("Text", "Shader Icon");
			menu.AddItem(shaderTextContent, false, () => SetShader(MixtureEditorUtils.CreateNewShaderText(owner.graph, title, dim), shaderCreationUI, shaderField));
			menu.ShowAsContext();
		}
		
		void ResetMaterialPropertyToDefault(PortView pv)
		{
			foreach (var p in shaderNode.ListMaterialProperties(null))
			{
				if (pv.portData.identifier == p.identifier)
					shaderNode.ResetMaterialPropertyToDefault(shaderNode.material, p.identifier);
			}
		}

		protected void SetShader(Shader newShader, VisualElement shaderCreationUI, ObjectField shaderField)
		{
			owner.RegisterCompleteObjectUndo("Updated Shader of ShaderNode");
			shaderNode.shader = newShader;
			shaderField.value = newShader;
			shaderNode.material.shader = newShader;
			UpdateShaderCreationUI(shaderCreationUI, shaderField);

			title = newShader?.name ?? "New Shader";

			// We fore the update of node ports
			ForceUpdatePorts();

			shaderNode.ValidateShader();
		}

		void MaterialGUI(bool fromInspector, VisualElement shaderCreationUI, ObjectField shaderField)
		{
			if (GetMaterialHash(shaderNode.material) != materialHash)
			{
				NotifyNodeChanged();
				materialHash = GetMaterialHash(shaderNode.material);
			}

			// Update the GUI when shader is modified
			if (MaterialPropertiesGUI(shaderNode.material, fromInspector))
			{
				// Delay execution to sync with UIElement layout
				schedule.Execute(() => 
				{
					UpdateShaderCreationUI(shaderCreationUI, shaderField);
					ForceUpdatePorts();
				}).ExecuteLater(1);
			}
		}

		public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
		{
			base.BuildContextualMenu(evt);

			if (shaderNode.shader != null)
			{
				evt.menu.InsertAction(2, "📜 Open Shader Code", (e) => {
					AssetDatabase.OpenAsset(shaderNode.shader);
				});
			}
		}

		public override void OnRemoved() => owner.graph.RemoveObjectFromGraph(shaderNode.material);
	}
}