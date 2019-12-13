using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using GraphProcessor;
using UnityEngine.Rendering;

namespace Mixture
{
	[NodeCustomEditor(typeof(OutputNode))]
	public class OutputNodeView : MixtureNodeView
	{
		VisualElement	shaderCreationUI;
		VisualElement	materialEditorUI;
		MaterialEditor	materialEditor;
		protected OutputNode	outputNode;
		protected MixtureGraph	graph;

		// Debug fields
		ObjectField		debugCustomRenderTextureField;

		protected override bool hasPreview => true;

		public override void Enable()
		{
            outputNode = nodeTarget as OutputNode;
            graph = owner.graph as MixtureGraph;

            BuildOutputNodeSettings();

            base.Enable();

			outputNode.onTempRenderTextureUpdated += UpdatePreviewImage;
			graph.onOutputTextureUpdated += UpdatePreviewImage;

			// Clear the input when disconnecting it:
			onPortDisconnected += _ => outputNode.input = null;

			InitializeDebug();
		}

        protected virtual void BuildOutputNodeSettings()
        {
			if (graph.outputTexture.dimension == TextureDimension.Tex2D)
				AddCompressionSettings();

            if (!graph.isRealtime)
            {
                controlsContainer.Add(new Button(SaveMasterTexture)
                {
                    text = "Save"
                });
            }

        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
		{
			base.BuildContextualMenu(evt);
			
			string mode = graph.isRealtime ? "Static" : "Realtime";
			evt.menu.AppendSeparator();
			evt.menu.AppendAction($"Convert Mixture to {mode}", _ => MixtureEditorUtils.ToggleMode(graph), DropdownMenuAction.AlwaysEnabled);
		}

		void InitializeDebug()
		{
			outputNode.onProcessed += () => {
				debugCustomRenderTextureField.value = outputNode.tempRenderTexture;
			};

			debugCustomRenderTextureField = new ObjectField("Output")
			{
				value = outputNode.tempRenderTexture
			};
			
			debugContainer.Add(debugCustomRenderTextureField);
		}
		
		void AddCompressionSettings()
		{
			var formatField = new EnumField("Format", outputNode.compressionFormat);
			formatField.RegisterValueChangedCallback((e) => {
				owner.RegisterCompleteObjectUndo("Changed Compression Format");
				outputNode.compressionFormat = (MixtureCompressionFormat)e.newValue;
			});
			var qualityField = new EnumField("Quality", outputNode.compressionQuality);
			qualityField.RegisterValueChangedCallback((e) => {
				owner.RegisterCompleteObjectUndo("Changed Compression Quality");
				outputNode.compressionQuality = (MixtureCompressionQuality)e.newValue;
			});

			if (!outputNode.enableCompression)
			{
				qualityField.ToggleInClassList("Hidden");
				formatField.ToggleInClassList("Hidden");
			}
			
			var enabledField = new Toggle("Compression") { value = outputNode.enableCompression };
			enabledField.RegisterValueChangedCallback((e) => {
				owner.RegisterCompleteObjectUndo("Toggled Compression");
				qualityField.ToggleInClassList("Hidden");
				formatField.ToggleInClassList("Hidden");
				outputNode.enableCompression = e.newValue;
			});

			controlsContainer.Add(enabledField);
			controlsContainer.Add(formatField);
			controlsContainer.Add(qualityField);
		}

		void UpdatePreviewImage() => CreateTexturePreview(previewContainer, outputNode, outputNode.currentSlice);

        protected void SaveMasterTexture()
        {
            graph.SaveMainTexture();
        }

	}
}
