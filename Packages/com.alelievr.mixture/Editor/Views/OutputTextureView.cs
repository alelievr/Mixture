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
        VisualElement   root;

        // Output elements
        VisualElement port;
        VisualElement portSettings;
        VisualElement portNameAndSettings;
        TextField portName;
        Button settingsButton;
        Button enableCompression;
        VisualElement compressionFields;
        EnumField compressionFormat;
        EnumField compressionQuality;
        Button enableMipMap;
        VisualElement mipmapFields;
        ObjectField mipmapShaderField;
        Button createMipMapShaderButton;

        // state:
        bool settingsState;

        public OutputTextureView()
        {
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
            portName = this.Q("PortNameField") as TextField;
            settingsButton = this.Q("SettingsButton") as Button;
            enableCompression = this.Q("EnableCompression") as Button;
            compressionFormat = this.Q("CompressionFormat") as EnumField;
            compressionQuality = this.Q("CompressionQuality") as EnumField;
            enableMipMap = this.Q("EnableMipMap") as Button;
            compressionFields = this.Q("CompressionFields");
            mipmapFields = this.Q("MipMapFields");
        }

        void InitializeView()
        {
            // Register callbacks
            settingsButton.clicked += () => {
                if (settingsButton.ClassListContains("ActiveButton"))
                {
                    settingsButton.RemoveFromClassList("ActiveButton");
                    portSettings.style.display = DisplayStyle.None;
                }
                else
                {
                    settingsButton.AddToClassList("ActiveButton");
                    portSettings.style.display = DisplayStyle.Flex;
                }
            };

            // Initial view state
            portSettings.style.display = DisplayStyle.None;
            compressionFields.style.display = DisplayStyle.None;
            mipmapFields.style.display = DisplayStyle.None;
        }

        public void MovePort(PortView portView)
        {
            portView.RemoveFromHierarchy();
            // move the port into our container
            port.Add(portView);
        }

    }
}
