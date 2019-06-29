using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
	public class MixtureNodeView : BaseNodeView
	{
		protected VisualElement propertyEditorUI;
		protected VisualElement rtSettingsContainerUI;
        protected VisualElement previewContainer;

        protected new MixtureGraphView  owner => base.owner as MixtureGraphView;
		protected new MixtureNode       nodeTarget => base.nodeTarget as MixtureNode;

		Dictionary< Material, MaterialProperty[] >  oldMaterialProperties = new Dictionary<Material, MaterialProperty[]>();
		Dictionary< Material, MaterialEditor >      materialEditors = new Dictionary<Material, MaterialEditor>();

		protected virtual string header {get { return string.Empty; } }
		protected virtual bool showPreview { get { return false; } }

		const string stylesheetName = "MixtureCommon";

        EnumField outputWidthMode;
		EnumField outputHeightMode;
		EnumField outputDepthMode;
		EnumField outputDimension;
		EnumField outputFormat;
		
        IntegerField outputWidth;
		FloatField outputWidthPercentage;
        IntegerField outputHeight;
		FloatField outputHeightPercentage;
        IntegerField outputDepth;
		FloatField outputDepthPercentage;
		

        public override void Enable()
		{
            var mixtureNode = nodeTarget as MixtureNode;
			var stylesheet = Resources.Load<StyleSheet>(stylesheetName);
			if(!styleSheets.Contains(stylesheet))
				styleSheets.Add(stylesheet);
			
			propertyEditorUI = new VisualElement();
			controlsContainer.Add(propertyEditorUI);

			propertyEditorUI.AddToClassList("PropertyEditorUI");
			controlsContainer.AddToClassList("ControlsContainer");

			rtSettingsContainerUI = GetRTSettingsContainer(mixtureNode);
			rtSettingsContainerUI.AddToClassList("PropertyEditorUI");
			controlsContainer.Add(rtSettingsContainerUI);

			if(header != string.Empty)
			{
				var title = new Label(header);
				title.AddToClassList("PropertyEditorTitle");
				propertyEditorUI.Add(title);
			}

			if(showPreview)
			{
                CreateTexturePreview(previewContainer, mixtureNode.previewTexture); // TODO : Add Slice Preview
                controlsContainer.Add(previewContainer);

            }

            titleButtonContainer.Add(new Button(ToggleSettings) { text = "S" });
            propertyEditorUI.style.display = DisplayStyle.Flex;
            rtSettingsContainerUI.style.display = DisplayStyle.None;
        }

		void ToggleSettings()
		{
            propertyEditorUI.style.display = propertyEditorUI.style.display == DisplayStyle.None ? DisplayStyle.Flex : DisplayStyle.None;
            rtSettingsContainerUI.style.display = rtSettingsContainerUI.style.display == DisplayStyle.None ? DisplayStyle.Flex : DisplayStyle.None;
        }

		bool CheckPropertyChanged(Material material, MaterialProperty[] properties)
		{
			bool propertyChanged = false;
			MaterialProperty[]  oldProperties;
			oldMaterialProperties.TryGetValue(material, out oldProperties);

			if (oldProperties != null)
			{
				// Check if shader was changed (new/deleted properties)
				if (properties.Length != oldProperties.Length)
				{
					propertyChanged = true;
				}
				else
				{
					for (int i = 0; i < properties.Length; i++)
					{
						if (properties[i].type != oldProperties[i].type)
							propertyChanged = true;
						if (properties[i].displayName != oldProperties[i].displayName)
							propertyChanged = true;
						if (properties[i].flags != oldProperties[i].flags)
							propertyChanged = true;
						if (properties[i].name != oldProperties[i].name)
							propertyChanged = true;
					}
				}
			}

			oldMaterialProperties[material] = MaterialEditor.GetMaterialProperties(new []{material});

			return propertyChanged;
		}

		void SetVisible(VisualElement element, bool visible)
		{
            element.style.display = visible? DisplayStyle.Flex: DisplayStyle.None;
        }

		void UpdateFieldVisibility(MixtureNode node)
		{
            var editFlags = node.rtSettings.editFlags;
            var rtSettings = node.rtSettings;
            SetVisible(outputWidthMode, rtSettings.CanEdit(EditFlags.WidthMode));
            SetVisible(outputHeightMode, rtSettings.CanEdit(EditFlags.HeightMode));
            SetVisible(outputDepthMode, rtSettings.CanEdit(EditFlags.DepthMode));
            SetVisible(outputWidth, rtSettings.CanEdit(EditFlags.Width) && node.rtSettings.widthMode == OutputSizeMode.Fixed);
            SetVisible(outputWidthPercentage, rtSettings.CanEdit(EditFlags.Width) && node.rtSettings.widthMode == OutputSizeMode.PercentageOfOutput);
			SetVisible(outputHeight, rtSettings.CanEdit(EditFlags.Height) && node.rtSettings.heightMode == OutputSizeMode.Fixed);
            SetVisible(outputHeightPercentage, rtSettings.CanEdit(EditFlags.Height) && node.rtSettings.heightMode == OutputSizeMode.PercentageOfOutput);
			SetVisible(outputDepth, rtSettings.CanEdit(EditFlags.Depth) && node.rtSettings.depthMode == OutputSizeMode.Fixed);
            SetVisible(outputDepthPercentage, rtSettings.CanEdit(EditFlags.Depth) && node.rtSettings.depthMode == OutputSizeMode.PercentageOfOutput);
		}

		protected VisualElement GetRTSettingsContainer(MixtureNode node)
		{
			var graph = owner.graph as MixtureGraph;
			var container = new VisualElement();
			var title = new Label("Node Output Settings");
			title.AddToClassList("PropertyEditorTitle");
			container.Add(title);

			// Size Modes
			outputWidthMode = new EnumField(node.rtSettings.widthMode) {
				label = "Width Mode",
			};
			outputWidthMode.RegisterValueChangedCallback(e => {
				owner.RegisterCompleteObjectUndo("Updated Texture Dimension " + e.newValue);
				node.rtSettings.widthMode = (OutputSizeMode)e.newValue;
				graph.UpdateOutputTexture();
                UpdateFieldVisibility(node);
            });
			container.Add(outputWidthMode);

			outputHeightMode = new EnumField(node.rtSettings.heightMode) {
				label = "Height Mode",
			};
			outputHeightMode.RegisterValueChangedCallback(e => {
				owner.RegisterCompleteObjectUndo("Updated Texture Dimension " + e.newValue);
				node.rtSettings.heightMode = (OutputSizeMode)e.newValue;
				graph.UpdateOutputTexture();
				UpdateFieldVisibility(node);
            });
			container.Add(outputHeightMode);

			outputDepthMode = new EnumField(node.rtSettings.depthMode) {
				label = "Depth Mode",
			};
			outputDepthMode.RegisterValueChangedCallback(e => {
				owner.RegisterCompleteObjectUndo("Updated Texture Dimension " + e.newValue);
				node.rtSettings.depthMode = (OutputSizeMode)e.newValue;
				graph.UpdateOutputTexture();
                UpdateFieldVisibility(node);
            });
			container.Add(outputDepthMode);

			outputWidth = new IntegerField()
			{
				value = node.rtSettings.width,
				label = "Width"
			};
			outputWidth.RegisterValueChangedCallback(e =>
			{
				owner.RegisterCompleteObjectUndo("Updated Width " + e.newValue);
				node.rtSettings.width = e.newValue;
				graph.UpdateOutputTexture();
			});
			container.Add(outputWidth);

			outputWidthPercentage = new FloatField()
			{
				value = node.rtSettings.widthPercent,
				label = "Width Percentage"
			};
			outputWidthPercentage.RegisterValueChangedCallback(e =>
			{
				owner.RegisterCompleteObjectUndo("Updated Width " + e.newValue);
				node.rtSettings.widthPercent = e.newValue;
				graph.UpdateOutputTexture();
			});
			container.Add(outputWidthPercentage);

			outputHeight = new IntegerField()
			{
				value = node.rtSettings.height,
				label = "Height"
			};
			outputHeight.RegisterValueChangedCallback(e =>
			{
				owner.RegisterCompleteObjectUndo("Updated Height " + e.newValue);
				node.rtSettings.height = e.newValue;
				graph.UpdateOutputTexture();
			});
			container.Add(outputHeight);

			outputHeightPercentage = new FloatField()
			{
				value = node.rtSettings.heightPercent,
				label = "Height Percentage"
			};
			outputHeightPercentage.RegisterValueChangedCallback(e =>
			{
				owner.RegisterCompleteObjectUndo("Updated Width " + e.newValue);
				node.rtSettings.heightPercent = e.newValue;
				graph.UpdateOutputTexture();
			});
			container.Add(outputHeightPercentage);

			outputDepth = new IntegerField()
			{
				value = node.rtSettings.depth,
				label = "Depth"
			};
			outputDepth.RegisterValueChangedCallback(e =>
			{
				owner.RegisterCompleteObjectUndo("Updated Depth " + e.newValue);
				node.rtSettings.depth = e.newValue;
				graph.UpdateOutputTexture();
			});
			container.Add(outputDepth);

			outputDepthPercentage = new FloatField()
			{
				value = node.rtSettings.depthPercent,
				label = "Depth Percentage"
			};
			outputDepthPercentage.RegisterValueChangedCallback(e =>
			{
				owner.RegisterCompleteObjectUndo("Updated Width " + e.newValue);
				node.rtSettings.depthPercent = e.newValue;
				graph.UpdateOutputTexture();
			});
			container.Add(outputDepthPercentage);

			outputDimension = new EnumField(node.rtSettings.dimension) {
				label = "Dimension",
			};
			outputDimension.RegisterValueChangedCallback(e => {
				owner.RegisterCompleteObjectUndo("Updated Texture Dimension " + e.newValue);
				node.rtSettings.dimension = (OutputDimension)e.newValue;
				graph.UpdateOutputTexture();
			});

			outputFormat = new EnumField(node.rtSettings.targetFormat) {
				label = "Pixel Format",
			};
			outputFormat.RegisterValueChangedCallback(e => {
				owner.RegisterCompleteObjectUndo("Updated Graphics Format " + e.newValue);
				node.rtSettings.targetFormat = (OutputFormat)e.newValue;
				graph.UpdateOutputTexture();
			});

			container.Add(outputDimension);
			container.Add(outputFormat);

			UpdateFieldVisibility(node);
			return container;
		}

		// Custom property draw, we don't want things that are connected to an edge or useless like the render queue
		protected bool MaterialPropertiesGUI(Material material)
		{
			if (material == null || material.shader == null)
				return false;

			MaterialProperty[] properties = MaterialEditor.GetMaterialProperties(new []{material});
			var portViews = GetPortViewsFromFieldName(nameof(ShaderNode.materialInputs));

			MaterialEditor  editor;
			if (!materialEditors.TryGetValue(material, out editor))
				editor = materialEditors[material] = Editor.CreateEditor(material) as MaterialEditor;

			bool propertiesChanged = CheckPropertyChanged(material, properties);

			foreach (var property in properties)
			{
				if ((property.flags & (MaterialProperty.PropFlags.HideInInspector | MaterialProperty.PropFlags.PerRendererData)) != 0)
					continue;

				// Retrieve the port view from the property name
				var portView = portViews?.FirstOrDefault(p => p.portData.identifier == property.name);
				if (portView != null && portView.connected)
					continue;

				float h = editor.GetPropertyHeight(property, property.displayName);
				Rect r = EditorGUILayout.GetControlRect(true, h, EditorStyles.layerMaskField);

				editor.ShaderProperty(r, property, property.displayName);
			}

			return propertiesChanged;
		}

		protected void CreateTexturePreview(VisualElement previewContainer, Texture texture, int currentSlice = 0)
		{
			previewContainer.Clear();

			if (texture == null)
				return;

			switch (texture.dimension)
			{
				case TextureDimension.Tex2D:
					CreateTexture2DPreview(previewContainer, texture);
					break;
				case TextureDimension.Tex2DArray:
					CreateTexture2DArrayPreview(previewContainer, texture, currentSlice);
					break;
				// TODO: Texture3D
				default:
					Debug.LogError(texture + " is not a supported type for preview");
					return;
			}
		}

		void CreateTexture2DPreview(VisualElement previewContainer, Texture texture)
		{
			var previewImage = new Image
			{
				image = texture,
				scaleMode = ScaleMode.StretchToFill,
			};
			previewContainer.Add(previewImage);
		}

		void CreateTexture2DArrayPreview(VisualElement previewContainer, Texture texture, int currentSlice)
		{
			var previewSliceIndex = new SliderInt(0, TextureUtils.GetSliceCount(texture) - 1)
			{
				label = "Slice",
				value = currentSlice,
			};
			var previewImageSlice = new IMGUIContainer(() => {
				// square image:
				int size = (int)previewContainer.parent.style.width.value.value;
				var rect = EditorGUILayout.GetControlRect(GUILayout.Height(size), GUILayout.Width(size));
				MixtureUtils.textureArrayPreviewMaterial.SetTexture("_TextureArray", texture);
				MixtureUtils.textureArrayPreviewMaterial.SetFloat("_Slice", currentSlice);
				EditorGUI.DrawPreviewTexture(rect, Texture2D.whiteTexture, MixtureUtils.textureArrayPreviewMaterial);
			});
			previewSliceIndex.RegisterValueChangedCallback((ChangeEvent< int > a) => {
				currentSlice = a.newValue;
			});
			previewContainer.Add(previewSliceIndex);
			previewContainer.Add(previewImageSlice);
		}
	}
}