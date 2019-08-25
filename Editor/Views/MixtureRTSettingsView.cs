using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using GraphProcessor;
using System.Linq;
using System;

namespace Mixture
{
	public class MixtureRTSettingsView : VisualElement
	{
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

		Toggle doubleBuffered;

        Action  onChanged;

        public MixtureRTSettingsView(MixtureNode node, MixtureGraphView owner)
        {
			var graph = owner.graph as MixtureGraph;
			var title = new Label("Node Output Settings");
			title.AddToClassList("PropertyEditorTitle");
			this.Add(title);

			// Size Modes
			outputWidthMode = new EnumField(node.rtSettings.widthMode) {
				label = "Width Mode",
			};
			outputWidthMode.RegisterValueChangedCallback(e => {
				owner.RegisterCompleteObjectUndo("Updated Texture Dimension " + e.newValue);
				node.rtSettings.widthMode = (OutputSizeMode)e.newValue;
                onChanged?.Invoke();
                UpdateFieldVisibility(node);
            });
			this.Add(outputWidthMode);

			outputHeightMode = new EnumField(node.rtSettings.heightMode) {
				label = "Height Mode",
			};
			outputHeightMode.RegisterValueChangedCallback(e => {
				owner.RegisterCompleteObjectUndo("Updated Texture Dimension " + e.newValue);
				node.rtSettings.heightMode = (OutputSizeMode)e.newValue;
                onChanged?.Invoke();
				UpdateFieldVisibility(node);
            });
			this.Add(outputHeightMode);

			outputDepthMode = new EnumField(node.rtSettings.depthMode) {
				label = "Depth Mode",
			};
			outputDepthMode.RegisterValueChangedCallback(e => {
				owner.RegisterCompleteObjectUndo("Updated Texture Dimension " + e.newValue);
				node.rtSettings.depthMode = (OutputSizeMode)e.newValue;
                onChanged?.Invoke();
                UpdateFieldVisibility(node);
            });
			this.Add(outputDepthMode);

			outputWidth = new IntegerField()
			{
				value = node.rtSettings.width,
				label = "Width",
                isDelayed = true,
			};
			outputWidth.RegisterValueChangedCallback(e =>
			{
				owner.RegisterCompleteObjectUndo("Updated Width " + e.newValue);
				node.rtSettings.width = e.newValue;
                onChanged?.Invoke();
			});
			this.Add(outputWidth);

			outputWidthPercentage = new FloatField()
			{
				value = node.rtSettings.widthPercent,
				label = "Width Percentage",
                isDelayed = true,
			};
			outputWidthPercentage.RegisterValueChangedCallback(e =>
			{
				owner.RegisterCompleteObjectUndo("Updated Width " + e.newValue);
				node.rtSettings.widthPercent = e.newValue;
                onChanged?.Invoke();
			});
			this.Add(outputWidthPercentage);

			outputHeight = new IntegerField()
			{
				value = node.rtSettings.height,
				label = "Height",
                isDelayed = true,
			};
			outputHeight.RegisterValueChangedCallback(e =>
			{
				owner.RegisterCompleteObjectUndo("Updated Height " + e.newValue);
				node.rtSettings.height = e.newValue;
                onChanged?.Invoke();
			});
			this.Add(outputHeight);

			outputHeightPercentage = new FloatField()
			{
				value = node.rtSettings.heightPercent,
				label = "Height Percentage",
                isDelayed = true,
			};
			outputHeightPercentage.RegisterValueChangedCallback(e =>
			{
				owner.RegisterCompleteObjectUndo("Updated Width " + e.newValue);
				node.rtSettings.heightPercent = e.newValue;
                onChanged?.Invoke();
			});
			this.Add(outputHeightPercentage);

			outputDepth = new IntegerField()
			{
				value = node.rtSettings.sliceCount,
				label = "Depth",
                isDelayed = true,
			};
			outputDepth.RegisterValueChangedCallback(e =>
			{
				owner.RegisterCompleteObjectUndo("Updated Depth " + e.newValue);
				node.rtSettings.sliceCount = e.newValue;
                onChanged?.Invoke();
			});
			this.Add(outputDepth);

			outputDepthPercentage = new FloatField()
			{
				value = node.rtSettings.depthPercent,
				label = "Depth Percentage",
                isDelayed = true,
			};
			outputDepthPercentage.RegisterValueChangedCallback(e =>
			{
				owner.RegisterCompleteObjectUndo("Updated Width " + e.newValue);
				node.rtSettings.depthPercent = e.newValue;
                onChanged?.Invoke();
			});
			this.Add(outputDepthPercentage);

			outputDimension = new EnumField(node.rtSettings.dimension) {
				label = "Dimension",
			};
			outputDimension.RegisterValueChangedCallback(e => {
				owner.RegisterCompleteObjectUndo("Updated Texture Dimension " + e.newValue);
				node.rtSettings.dimension = (OutputDimension)e.newValue;
                onChanged?.Invoke();
			});

			outputFormat = new EnumField(node.rtSettings.targetFormat) {
				label = "Pixel Format",
			};
			outputFormat.RegisterValueChangedCallback(e => {
				owner.RegisterCompleteObjectUndo("Updated Graphics Format " + e.newValue);
				node.rtSettings.targetFormat = (OutputFormat)e.newValue;
                onChanged?.Invoke();
			});

			this.Add(outputDimension);
			this.Add(outputFormat);

			UpdateFieldVisibility(node);

			if (owner.graph.isRealtime)
				AddRealtimeFields(node, owner);
        }

		void AddRealtimeFields(MixtureNode node, MixtureGraphView owner)
		{
			doubleBuffered = new Toggle("Double Buffered") {
				value = node.rtSettings.doubleBuffered,
			};
			doubleBuffered.RegisterValueChangedCallback(e => {
				owner.RegisterCompleteObjectUndo("Set Double Buffered " + e.newValue);
				node.rtSettings.doubleBuffered = e.newValue;
                onChanged?.Invoke();
			});

			Add(doubleBuffered);
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

        public void RegisterChangedCallback(Action callback) => onChanged = callback;
    }
}
