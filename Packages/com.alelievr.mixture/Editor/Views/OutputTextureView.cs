using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine.Rendering;
using GraphProcessor;

namespace Mixture
{
	public class OutputTextureView : VisualElement
    {
        internal PortView       portView;

        VisualElement           root;
        OutputTextureSettings   targetSettings;
        MixtureGraphView        graphView;
        OutputNodeView          nodeView;
        OutputNode              node;

        // Output elements
        VisualElement port;
        VisualElement portSettings;
        VisualElement portNameAndSettings;
        TextField portNameField;
        Button settingsButton;
        Toggle enableCompression;
        VisualElement compressionFields;
        EnumField compressionFormat;
        EnumField compressionQuality;
        Toggle enableMipMap;
        VisualElement mipmapFields;
        ObjectField mipmapShaderField;
        Button createMipMapShaderButton;
        Label portName;
        Button removeOutputButton;
        Toggle enableConversion;
        EnumField conversionFormat;
        VisualElement conversionSettings;
        VisualElement mipmapSettings;
        VisualElement sRGBSettings;
        Toggle sRGB;

        // state:
        bool settingsState;

        public OutputTextureView(MixtureGraphView graphView, OutputNodeView nodeView, OutputTextureSettings targetSettings)
        {
            this.nodeView = nodeView;
            node = nodeView.nodeTarget as OutputNode;
            this.targetSettings = targetSettings;
            this.graphView = graphView;

            root = Resources.Load<VisualTreeAsset>("UI Blocks/OutputTexture").CloneTree();
            Add(root);

            LoadOutputElements();
            InitializeView();
            RefreshSettings();
        }

        void LoadOutputElements()
        {
            port = this.Q("Port");
            portSettings = this.Q("PortSettings");
            portNameAndSettings = this.Q("PortNameAndSettings");
            portNameField = this.Q("PortNameField") as TextField;
            portName = this.Q("PortName") as Label;
            settingsButton = this.Q("SettingsButton") as Button;
            enableCompression = this.Q("EnableCompression") as Toggle;
            compressionFormat = this.Q("CompressionFormat") as EnumField;
            compressionQuality = this.Q("CompressionQuality") as EnumField;
            enableMipMap = this.Q("EnableMipMap") as Toggle;
            compressionFields = this.Q("CompressionFields");
            mipmapFields = this.Q("MipMapFields");
            createMipMapShaderButton = this.Q("NewMipMapShader") as Button;
            mipmapShaderField = this.Q("ShaderField") as ObjectField;
            removeOutputButton = this.Q("RemoveButton") as Button;
            conversionSettings = this.Q("ConversionSettings");
            conversionFormat = this.Q("ConversionFormat") as EnumField;
            enableConversion = this.Q("EnableConversion") as Toggle;
            mipmapSettings = this.Q("MipMapSettings");
            sRGBSettings = this.Q("SRGBSettings");
            sRGB = this.Q("sRGB") as Toggle;
        }

        bool supportCustomMipMaps => node.rtSettings.GetTextureDimension(graphView.graph) == TextureDimension.Tex2D;

        void InitializeView()
        {
            // Register callbacks
            settingsButton.clicked += () => {
                if (settingsButton.ClassListContains("ActiveButton"))
                {
                    settingsButton.RemoveFromClassList("ActiveButton");
                    portSettings.style.display = DisplayStyle.None;
                    portNameField.style.display = DisplayStyle.None;
                }
                else
                {
                    settingsButton.AddToClassList("ActiveButton");
                    portSettings.style.display = DisplayStyle.Flex;
                    portNameField.style.display = DisplayStyle.Flex;
                }
            };

            portNameField.RegisterValueChangedCallback(name => {
                graphView.RegisterCompleteObjectUndo("Change output name");
                var uniqueName = ObjectNames.GetUniqueName(node.outputTextureSettings.Select(o => o.name).ToArray(), name.newValue);
                targetSettings.name = uniqueName;
                portName.text = uniqueName;

#if UNITY_EDITOR
			if (graphView.graph.isRealtime)
				graphView.graph.UpdateRealtimeAssetsOnDisk();
#endif
            });
            portNameField.value = targetSettings.name;
            portName.text = targetSettings.name;

            enableCompression.RegisterValueChangedCallback(enabled => {
                graphView.RegisterCompleteObjectUndo($"Change {targetSettings.name} compression");
                var textureDim = node.rtSettings.GetTextureDimension(graphView.graph);

                if (textureDim == TextureDimension.Tex2D || textureDim == TextureDimension.Cube)
                {
                    targetSettings.enableCompression = enabled.newValue;
                    if (enabled.newValue)
                        compressionFields.style.display = DisplayStyle.Flex;
                    else
                        compressionFields.style.display = DisplayStyle.None;
                }
                else
                {
                    Debug.LogError("Compression is not yet supported for " + textureDim);
                    enableCompression.SetValueWithoutNotify(false);
                }
            });
            enableCompression.value = targetSettings.enableCompression;

            compressionFormat.RegisterValueChangedCallback(format => {
                targetSettings.compressionFormat = (TextureFormat)format.newValue;
            });
            compressionQuality.RegisterValueChangedCallback(quality => {
                targetSettings.compressionQuality = (MixtureCompressionQuality)quality.newValue;
            });

            compressionFormat.SetValueWithoutNotify(targetSettings.compressionFormat);
            compressionQuality.SetValueWithoutNotify(targetSettings.compressionQuality);

			createMipMapShaderButton.clicked += MixtureAssetCallbacks.CreateCustomMipMapShaderGraph;
			// TODO: assign the created shader when finished

			mipmapShaderField.objectType = typeof(Shader);
			mipmapShaderField.value = targetSettings.customMipMapShader;
			createMipMapShaderButton.style.display = targetSettings.customMipMapShader != null ? DisplayStyle.None : DisplayStyle.Flex;
			mipmapShaderField.RegisterValueChangedCallback(e => {
				graphView.RegisterCompleteObjectUndo("Changed Custom Mip Map Shader");
				targetSettings.customMipMapShader = e.newValue as Shader;
				createMipMapShaderButton.style.display = e.newValue != null ? DisplayStyle.None : DisplayStyle.Flex;;
			});

			enableMipMap.RegisterValueChangedCallback(e => {
				targetSettings.hasMipMaps = e.newValue;
                if (supportCustomMipMaps)
                    mipmapFields.style.display = targetSettings.hasMipMaps ? DisplayStyle.Flex : DisplayStyle.None;
                
                // Processing the graph to update the previews with the new mipmaps
                graphView.ProcessGraph(); 
			});
            enableMipMap.value = targetSettings.hasMipMaps;

            removeOutputButton.clicked += () => {
                node.RemoveTextureOutput(targetSettings);
                nodeView.ForceUpdatePorts();
            };

            enableConversion.RegisterValueChangedCallback(e => {
				graphView.RegisterCompleteObjectUndo("Changed Conversion value");
                targetSettings.enableConversion = e.newValue;
                conversionFormat.style.display = e.newValue ? DisplayStyle.Flex : DisplayStyle.None;
            });
            enableConversion.value = targetSettings.enableConversion;

            conversionFormat.RegisterValueChangedCallback(e => {
				graphView.RegisterCompleteObjectUndo("Changed Conversion Format");
                targetSettings.conversionFormat = (ConversionFormat)e.newValue;
            });
            conversionFormat.value = targetSettings.conversionFormat;

            sRGB.RegisterValueChangedCallback(e => {
				graphView.RegisterCompleteObjectUndo("Changed sRGB");
                targetSettings.sRGB = e.newValue;

                // when updating sRGB flag, we need to process the graph to update the final copy material
               graphView.ProcessGraph(); 
            });
            sRGB.value = targetSettings.sRGB;

            // Initial view state
            portSettings.style.display = DisplayStyle.None;
            compressionFields.style.display = targetSettings.enableCompression ? DisplayStyle.Flex : DisplayStyle.None;
			mipmapFields.style.display = targetSettings.hasMipMaps ? DisplayStyle.Flex : DisplayStyle.None;
            portNameField.style.display = DisplayStyle.None;
            conversionFormat.style.display = targetSettings.enableConversion ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public void RefreshSettings()
        {
            var dimension = node.rtSettings.GetTextureDimension(graphView.graph);

            if (graphView.graph.isRealtime)
            {
                // In realtime we don't support compression or conversion
                compressionFields.style.display = DisplayStyle.None;
                enableCompression.style.display = DisplayStyle.None;
                conversionSettings.style.display = DisplayStyle.None;
                mipmapSettings.style.display = DisplayStyle.Flex;

                mipmapFields.style.display = supportCustomMipMaps ? DisplayStyle.Flex : DisplayStyle.None;
            }
            else
            {
                switch (dimension)
                {
                    case TextureDimension.Tex2D:
                        // Tex2D supports compression + custom mip maps and we hide conversion settings (compression should be enough)
                        compressionFields.style.display = targetSettings.enableCompression ? DisplayStyle.Flex : DisplayStyle.None;
                        enableCompression.style.display = DisplayStyle.Flex;
                        conversionSettings.style.display = DisplayStyle.None;
                        mipmapSettings.style.display = DisplayStyle.Flex;
                        if (targetSettings.hasMipMaps)
                            mipmapFields.style.display = targetSettings.hasMipMaps ? DisplayStyle.Flex : DisplayStyle.None;
                        break;
                    case TextureDimension.Tex3D:
                        // Tex3D supports conversion but not compression, mipmap but not custom mipmaps.
                        compressionFields.style.display = DisplayStyle.None;
                        enableCompression.style.display = DisplayStyle.None;
                        conversionSettings.style.display = DisplayStyle.Flex;
                        mipmapSettings.style.display = DisplayStyle.Flex;
                        mipmapFields.style.display = DisplayStyle.None;
                        break;
                    case TextureDimension.Cube:
                        // Cubemaps supports compression and mipmaps but not custom mipmaps.
                        compressionFields.style.display = targetSettings.enableCompression ? DisplayStyle.Flex : DisplayStyle.None;
                        enableCompression.style.display = DisplayStyle.Flex;
                        conversionSettings.style.display = DisplayStyle.None;
                        mipmapSettings.style.display = DisplayStyle.Flex;
                        mipmapFields.style.display = DisplayStyle.None;
                        break;
                }
            }
        }

        public void MovePort(PortView portView)
        {
            this.portView = portView;
            var type = portView.Q("type");
            if (type != null)
            {
                type.style.position = Position.Absolute;
            }
            portView.RemoveFromHierarchy();
            // move the port into our container
            port.Add(portView);
        }
    }
}
