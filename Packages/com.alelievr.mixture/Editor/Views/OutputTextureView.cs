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
        }

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
            });
            portNameField.value = targetSettings.name;
            portName.text = targetSettings.name;

            enableCompression.RegisterValueChangedCallback(enabled => {
                graphView.RegisterCompleteObjectUndo($"Change {targetSettings.name} compression");
                var textureDim = node.rtSettings.GetTextureDimension(graphView.graph);

                if (textureDim == TextureDimension.Tex2D)
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
				mipmapFields.style.display = targetSettings.hasMipMaps ? DisplayStyle.Flex : DisplayStyle.None;
			});
            enableMipMap.value = targetSettings.hasMipMaps;

            removeOutputButton.clicked += () => {
                node.RemoveTextureOutput(targetSettings);
                nodeView.ForceUpdatePorts();
            };

            // Initial view state
            portSettings.style.display = DisplayStyle.None;
            compressionFields.style.display = targetSettings.enableCompression ? DisplayStyle.Flex : DisplayStyle.None;
			mipmapFields.style.display = targetSettings.hasMipMaps ? DisplayStyle.Flex : DisplayStyle.None;
            portNameField.style.display = DisplayStyle.None;
        }


        public void MovePort(PortView portView)
        {
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
