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
	public class MixtureSettingsView : VisualElement
	{
        // Keep a second version of enums from MixtureNode.cs without the Inherit fields for the graph settings:

        public enum GraphOutputDimension
        {
            Texture2D = TextureDimension.Tex2D,
            CubeMap = TextureDimension.Cube,
            Texture3D = TextureDimension.Tex3D,
        }

        public enum GraphOutputSizeMode
        {
            Absolute = OutputSizeMode.Absolute,
        }

        public enum GraphOutputPrecision
        {
            LDR				= OutputPrecision.LDR,
            Half			= OutputPrecision.Half,
            Full			= OutputPrecision.Full,
        }

        public enum GraphOutputChannel
        {
            RGBA = OutputChannel.RGBA,
            RG = OutputChannel.RG,
            R = OutputChannel.R,
        }

        public enum GraphOutputWrapMode
        {
            Repeat = OutputWrapMode.Repeat,
            Clamp = OutputWrapMode.Clamp,
            Mirror = OutputWrapMode.Mirror,
            MirrorOnce = OutputWrapMode.MirrorOnce,
        }

        public enum GraphOutputFilterMode
        {
            Point = OutputFilterMode.Point,
            Bilinear = OutputFilterMode.Bilinear,
            Trilinear = OutputFilterMode.Trilinear,
        }

        public const string headerStyleClass = "PropertyEditorHeader";
        Label sizeHeader;
        Label smpHeader;
        Label formatHeader;
        Label otherHeader;

        EnumField outputSizeMode;
		EnumField outputDimension;
		EnumField outputChannels;
		EnumField outputPrecision;
        EnumField wrapMode;
        EnumField filterMode;
        EnumField potSize;

        IntegerField outputWidth;
		FloatField outputWidthPercentage;
        IntegerField outputHeight;
		FloatField outputHeightPercentage;
        IntegerField outputDepth;
		FloatField outputDepthPercentage;

		Toggle doubleBuffered;

		EnumField refreshMode;
        FloatField period;

        event Action onChanged;

        MixtureGraphView    owner;
        MixtureSettings     settings;
        MixtureGraph        graph;
        string              title;
        bool                showInheritanceValue;

        // TODO: Avoid user to pick unavailable texture formats:
        enum SRGBOutputChannels
        {
            RGBA = OutputChannel.RGBA,
            // R = OutputChannel.R,
        }

        public MixtureSettingsView(MixtureSettings settings, MixtureGraphView owner, string title = "Node Output Settings", bool showInheritanceValue = true)
        {
            this.graph = owner.graph as MixtureGraph;
            this.settings = settings;
            this.owner = owner;
            this.title = title;
            this.showInheritanceValue = showInheritanceValue;

			var stylesheet = Resources.Load<StyleSheet>("MixtureCommon");
            styleSheets.Add(stylesheet);
            ReloadSettingsView();

            onChanged += ReloadSettingsView;
        }

        void ReloadSettingsView()
        {
            // Remove all old fields
            Clear();

            if (title != null)
            {
                var titleLabel = new Label(title);
                titleLabel.AddToClassList("PropertyEditorTitle");
                this.Add(titleLabel);
            }

            var dimension = settings.GetTextureDimension(graph);

            // Wrap and Filter Modes
            smpHeader = new Label("Sampler States");
            smpHeader.AddToClassList(headerStyleClass);
            this.Add(smpHeader);

            wrapMode = showInheritanceValue ? new EnumField(settings.wrapMode) : new EnumField((GraphOutputWrapMode)settings.wrapMode);
            wrapMode.label = "Wrap Mode";
            wrapMode.RegisterValueChangedCallback(e =>
            {
                owner.RegisterCompleteObjectUndo("Updated Wrap Mode " + e.newValue);
                settings.wrapMode = (OutputWrapMode)e.newValue;
                onChanged?.Invoke();
            });

            filterMode = showInheritanceValue ? new EnumField(settings.filterMode) : new EnumField((GraphOutputFilterMode)settings.filterMode);
            filterMode.label = "Filter Mode";
            filterMode.RegisterValueChangedCallback(e =>
            {
                owner.RegisterCompleteObjectUndo("Updated Filter Mode " + e.newValue);
                settings.filterMode = (OutputFilterMode)e.newValue;
                onChanged?.Invoke();
            });

            this.Add(wrapMode);
            this.Add(filterMode);

            // Size Modes
            sizeHeader = new Label("Size Properties");
            sizeHeader.AddToClassList(headerStyleClass);
            this.Add(sizeHeader);

            outputSizeMode = showInheritanceValue ? new EnumField(settings.sizeMode) : new EnumField((GraphOutputSizeMode)settings.sizeMode);
            outputSizeMode.label = "Size Mode";
            outputSizeMode.RegisterValueChangedCallback((EventCallback<ChangeEvent<Enum>>)(e => {
                owner.RegisterCompleteObjectUndo("Updated Size mode " + e.newValue);
                settings.sizeMode = (OutputSizeMode)e.newValue;
                onChanged?.Invoke();
                UpdateFieldVisibility(settings);
            }));
            this.Add(outputSizeMode);

            potSize = new EnumField(settings.potSize)
            {
                value = settings.potSize,
                label = "Resolution",
            };
            potSize.RegisterValueChangedCallback(e =>
            {
                owner.RegisterCompleteObjectUndo("Updated Size " + e.newValue);
                var size = (POTSize)e.newValue;
                settings.potSize = size;

                if (size != POTSize.Custom)
                {
                    settings.width = (int)size;
                    settings.height = (int)size;
                    settings.depth = (int)size;
                }
                else
                {
                    settings.width = outputWidth.value;
                    settings.height = outputHeight.value;
                    if (outputDepth != null)
                        settings.depth = outputDepth.value;
                }

                onChanged?.Invoke();
                UpdateFieldVisibility(settings);
            });

            this.Add(potSize);

            outputWidth = new IntegerField()
            {
                value = settings.width,
                label = "Width",
                isDelayed = true,
            };
            outputWidth.RegisterValueChangedCallback(e =>
            {
                owner.RegisterCompleteObjectUndo("Updated Width " + e.newValue);
                settings.width = e.newValue;
                onChanged?.Invoke();
            });
            this.Add(outputWidth);

            outputWidthPercentage = new FloatField()
            {
                value = settings.widthPercent,
                label = "Width Percentage",
                isDelayed = true,
            };
            outputWidthPercentage.RegisterValueChangedCallback(e =>
            {
                owner.RegisterCompleteObjectUndo("Updated Width " + e.newValue);
                settings.widthPercent = e.newValue;
                onChanged?.Invoke();
            });
            this.Add(outputWidthPercentage);

            if (dimension != TextureDimension.Cube)
            {
                outputHeight = new IntegerField()
                {
                    value = settings.height,
                    label = "Height",
                    isDelayed = true,
                };
                outputHeight.RegisterValueChangedCallback(e =>
                {
                    owner.RegisterCompleteObjectUndo("Updated Height " + e.newValue);
                    settings.height = e.newValue;
                    onChanged?.Invoke();
                });
                this.Add(outputHeight);

                outputHeightPercentage = new FloatField()
                {
                    value = settings.heightPercent,
                    label = "Height Percentage",
                    isDelayed = true,
                };
                outputHeightPercentage.RegisterValueChangedCallback(e =>
                {
                    owner.RegisterCompleteObjectUndo("Updated Height " + e.newValue);
                    settings.heightPercent = e.newValue;
                    onChanged?.Invoke();
                });
                this.Add(outputHeightPercentage);

                if (dimension == TextureDimension.Tex3D)
                {
                    outputDepth = new IntegerField()
                    {
                        value = settings.depth,
                        label = "Depth",
                        isDelayed = true,
                    };
                    outputDepth.RegisterValueChangedCallback(e =>
                    {
                        owner.RegisterCompleteObjectUndo("Updated Depth " + e.newValue);
                        settings.depth = e.newValue;
                        onChanged?.Invoke();
                    });
                    this.Add(outputDepth);

                    outputDepthPercentage = new FloatField()
                    {
                        value = settings.depthPercent,
                        label = "Depth Percentage",
                        isDelayed = true,
                    };
                    outputDepthPercentage.RegisterValueChangedCallback(e =>
                    {
                        owner.RegisterCompleteObjectUndo("Updated Depth " + e.newValue);
                        settings.depthPercent = e.newValue;
                        onChanged?.Invoke();
                    });
                    this.Add(outputDepthPercentage);
                }
            }

            // Dimension and Pixel Format
            formatHeader = new Label("Format");
            formatHeader.AddToClassList(headerStyleClass);
            this.Add(formatHeader);

            outputDimension = showInheritanceValue ? new EnumField(settings.dimension) : new EnumField((GraphOutputDimension)settings.dimension);
            outputDimension.label = "Dimension";
            outputDimension.RegisterValueChangedCallback(e => {
                owner.RegisterCompleteObjectUndo("Updated Texture Dimension " + e.newValue);
                // Check if the new texture is not too high res:
                settings.dimension = (OutputDimension)e.newValue;
                if (settings.dimension == OutputDimension.Texture3D)
                {
                    long pixelCount = settings.GetResolvedWidth(graph) * settings.GetHeight(graph) * settings.GetDepth(graph);

                    // Above 16M pixels in a texture3D, processing can take too long and crash the GPU when a conversion happen
                    if (pixelCount > 16777216)
                    {
                        settings.SetPOTSize(64);
                    }
                }
                onChanged?.Invoke();
            });

            outputChannels = showInheritanceValue ? new EnumField(settings.outputChannels) : new EnumField((GraphOutputChannel)settings.outputChannels);
            outputChannels.label = "Output Channels";
            outputChannels.RegisterValueChangedCallback(e => {
                owner.RegisterCompleteObjectUndo("Updated Output Channels " + e.newValue);
                settings.outputChannels = (OutputChannel)e.newValue;
                onChanged?.Invoke();
            });

            outputPrecision = showInheritanceValue ? new EnumField(settings.outputPrecision) : new EnumField((GraphOutputPrecision)settings.outputPrecision);
            outputPrecision.label = "Output Precision";
            outputPrecision.RegisterValueChangedCallback(e => {
                owner.RegisterCompleteObjectUndo("Updated Output Precision " + e.newValue);
                settings.outputPrecision = (OutputPrecision)e.newValue;
                // outputPrecision.Init();
                onChanged?.Invoke();
            });

			this.Add(outputDimension);
			this.Add(outputChannels);
			this.Add(outputPrecision);

            UpdateFieldVisibility(settings);

			if (owner.graph.type == MixtureGraphType.Realtime && showInheritanceValue)
            {
                // Realtime fields and refresh mode
                otherHeader = new Label("Other");
                otherHeader.AddToClassList(headerStyleClass);
                this.Add(otherHeader);

				AddRealtimeFields(owner);
            }
        }

		void AddRealtimeFields(MixtureGraphView owner)
		{
			doubleBuffered = new Toggle("Double Buffered") {
				value = settings.doubleBuffered,
			};
			doubleBuffered.RegisterValueChangedCallback(e => {
				owner.RegisterCompleteObjectUndo("Set Double Buffered " + e.newValue);
				settings.doubleBuffered = e.newValue;
                onChanged?.Invoke();
			});

			Add(doubleBuffered);

			refreshMode = new EnumField("Refresh Mode", settings.refreshMode);
			refreshMode.RegisterValueChangedCallback(e => {
				owner.RegisterCompleteObjectUndo("Set Refresh Mode " + e.newValue);
				settings.refreshMode = (RefreshMode)e.newValue;
                onChanged?.Invoke();
			});

			Add(refreshMode);

			period = new FloatField("Period") { value = settings.period };
			period.RegisterValueChangedCallback(e => {
				owner.RegisterCompleteObjectUndo("Set Period " + e.newValue);
				settings.period = e.newValue;
                onChanged?.Invoke();
			});

			Add(period);
		}
        
		void SetVisible(VisualElement element, bool visible)
		{
            if (element == null)
                return;

            element.style.display = visible? DisplayStyle.Flex: DisplayStyle.None;
        }

		void UpdateFieldVisibility(MixtureSettings settings)
		{
            SetVisible(sizeHeader, settings.CanEdit(EditFlags.Size));
            SetVisible(formatHeader, settings.CanEdit(EditFlags.Format));
            SetVisible(outputSizeMode, settings.CanEdit(EditFlags.SizeMode));
            SetVisible(potSize, settings.CanEdit(EditFlags.POTSize) && settings.sizeMode == OutputSizeMode.Absolute);
            SetVisible(outputWidth, settings.CanEdit(EditFlags.Width) && settings.sizeMode == OutputSizeMode.Absolute && (settings.potSize == POTSize.Custom || !settings.CanEdit(EditFlags.POTSize)));
            SetVisible(outputWidthPercentage, settings.CanEdit(EditFlags.Width) && settings.sizeMode == OutputSizeMode.ScaleOfParent);
			SetVisible(outputHeight, settings.CanEdit(EditFlags.Height) && settings.sizeMode == OutputSizeMode.Absolute && (settings.potSize == POTSize.Custom || !settings.CanEdit(EditFlags.POTSize)));
            SetVisible(outputHeightPercentage, settings.CanEdit(EditFlags.Height) && settings.sizeMode == OutputSizeMode.ScaleOfParent);
			SetVisible(outputDepth, settings.CanEdit(EditFlags.Depth) && settings.sizeMode == OutputSizeMode.Absolute && (settings.potSize == POTSize.Custom || !settings.CanEdit(EditFlags.POTSize)));
            SetVisible(outputDepthPercentage, settings.CanEdit(EditFlags.Depth) && settings.sizeMode == OutputSizeMode.ScaleOfParent);
            SetVisible(outputDimension, settings.CanEdit(EditFlags.Dimension));
            SetVisible(outputChannels, settings.CanEdit(EditFlags.TargetFormat));
            SetVisible(outputPrecision, settings.CanEdit(EditFlags.TargetFormat));

            // GraphicsFormatUtility.GetComponentCount((GraphicsFormat)settings.targetFormat);
            // GraphicsFormatUtility.Get((GraphicsFormat)settings.targetFormat);
            // GraphicsFormatUtility.GetBlockWidth()
        }

        public void RegisterChangedCallback(Action callback) => onChanged += callback;

		public void RefreshSettingsValues()
		{
            outputSizeMode?.SetValueWithoutNotify(settings.sizeMode);
            outputDimension?.SetValueWithoutNotify(settings.dimension);
            outputChannels?.SetValueWithoutNotify(settings.outputChannels);
            outputPrecision?.SetValueWithoutNotify(settings.outputPrecision);
            wrapMode?.SetValueWithoutNotify(settings.wrapMode);
            filterMode?.SetValueWithoutNotify(settings.filterMode);
            potSize?.SetValueWithoutNotify(settings.potSize);

            outputWidth?.SetValueWithoutNotify(settings.width);
            outputWidthPercentage?.SetValueWithoutNotify(settings.widthPercent);
            outputHeight?.SetValueWithoutNotify(settings.height);
            outputHeightPercentage?.SetValueWithoutNotify(settings.heightPercent);
            outputDepth?.SetValueWithoutNotify(settings.depth);
            outputDepthPercentage?.SetValueWithoutNotify(settings.depthPercent);

            doubleBuffered?.SetValueWithoutNotify(settings.doubleBuffered);

            refreshMode?.SetValueWithoutNotify(settings.refreshMode);
            period?.SetValueWithoutNotify(settings.period);
		}
    }
}
