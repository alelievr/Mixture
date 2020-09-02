using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using GraphProcessor;
using UnityEngine.Rendering;
using UnityEditor.Experimental.GraphView;

namespace Mixture
{
	[NodeCustomEditor(typeof(OutputNode))]
	public class OutputNodeView : MixtureNodeView
	{
		protected OutputNode	outputNode;
		protected MixtureGraph	graph;

		// Debug fields
		ObjectField		debugCustomRenderTextureField;

		// For now we only support custom mip maps for texture 2D
		bool supportsCustomMipMap => outputNode.hasMips && (TextureDimension)outputNode.rtSettings.dimension == TextureDimension.Tex2D;

		public override void Enable(bool fromInspector)
		{
			capabilities &= ~Capabilities.Deletable;
            outputNode = nodeTarget as OutputNode;
            graph = owner.graph as MixtureGraph;

            BuildOutputNodeSettings();

            base.Enable(fromInspector);

			// We don't need the code for removing the material because this node can't be removed
			if (outputNode.finalCopyMaterial != null && !owner.graph.IsObjectInGraph(outputNode.finalCopyMaterial))
				owner.graph.AddObjectToGraph(outputNode.finalCopyMaterial);

			outputNode.onTempRenderTextureUpdated += UpdatePreviewImage;
			graph.onOutputTextureUpdated += UpdatePreviewImage;

			// Clear the input when disconnecting it:
			onPortDisconnected += _ => outputNode.input = null;

			InitializeDebug();
		}

		protected override VisualElement CreateSettingsView()
		{
			var sv = base.CreateSettingsView();

			settingsView.RegisterChangedCallback(() => {
				// Reflect the changes on the graph output texture but not on the asset to avoid stalls.
				graph.UpdateOutputTexture(false);
			});

			return sv;
		}

        protected virtual void BuildOutputNodeSettings()
        {
			if (graph.outputTexture.dimension == TextureDimension.Tex2D)
				AddCompressionSettings();

            if (!graph.isRealtime)
            {
				AddCustomMipMapSettings();

                controlsContainer.Add(new Button(SaveMasterTexture)
                {
                    text = "Save"
                });
            }
        }

		void AddCustomMipMapSettings()
		{
			var customMipMapBlock = Resources.Load<VisualTreeAsset>("UI Blocks/CustomMipMap").CloneTree();

			var button = customMipMapBlock.Q("NewMipMapShader") as Button;
			button.clicked += MixtureAssetCallbacks.CreateCustomMipMapShaderGraph;
			// TODO: assign the created shader when finished

			var shaderField = customMipMapBlock.Q("ShaderField") as ObjectField;
			shaderField.objectType = typeof(Shader);
			shaderField.value = outputNode.customMipMapShader;
			button.style.display = outputNode.customMipMapShader != null ? DisplayStyle.None : DisplayStyle.Flex;
			shaderField.RegisterValueChangedCallback(e => {
				owner.RegisterCompleteObjectUndo("Changed Custom Mip Map Shader");
				outputNode.customMipMapShader = e.newValue as Shader;
				button.style.display = e.newValue != null ? DisplayStyle.None : DisplayStyle.Flex;;
			});

			var mipMapToggle = new Toggle("Has Mip Maps") { value = outputNode.hasMips};
			customMipMapBlock.style.display = supportsCustomMipMap ? DisplayStyle.Flex : DisplayStyle.None;
			mipMapToggle.RegisterValueChangedCallback(e => {
				outputNode.hasMips = e.newValue;
				customMipMapBlock.style.display = supportsCustomMipMap ? DisplayStyle.Flex : DisplayStyle.None;
				graph.UpdateOutputTexture(false);
			});

			controlsContainer.Add(mipMapToggle);
			controlsContainer.Add(customMipMapBlock);
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
				graph.UpdateOutputTexture(false);
			});
			var qualityField = new EnumField("Quality", outputNode.compressionQuality);
			qualityField.RegisterValueChangedCallback((e) => {
				owner.RegisterCompleteObjectUndo("Changed Compression Quality");
				outputNode.compressionQuality = (MixtureCompressionQuality)e.newValue;
				graph.UpdateOutputTexture(false);
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
				graph.UpdateOutputTexture(false);
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
