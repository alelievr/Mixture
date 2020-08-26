using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using GraphProcessor;
using UnityEditor.Experimental.GraphView;

namespace Mixture
{
    [NodeCustomEditor(typeof(ExternalOutputNode))]
    public class ExternalOutputNodeView : OutputNodeView
    {
        Button saveButton;
        Button updateButton;

		public override void Enable(bool fromInspector)
        {
            base.Enable(fromInspector);

            // We can delete external outputs
            capabilities |= Capabilities.Deletable;
        }
        
        protected override void BuildOutputNodeSettings()
        {
            var externalOutputNode = nodeTarget as ExternalOutputNode;
            var nodeSettings = new IMGUIContainer(() =>
            {
                if(externalOutputNode.asset == null)
                {
                    EditorGUILayout.HelpBox("This output has not been saved yet, please click Save As to save the output texture.", MessageType.Info);
                }
                else
                {
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.ObjectField("Asset", externalOutputNode.asset, typeof(Texture2D), false);
                    EditorGUI.EndDisabledGroup();
                }
                EditorGUI.BeginChangeCheck();
                var outputDimension = EditorGUILayout.EnumPopup("Output Dimension", externalOutputNode.externalOutputDimension);
                if (EditorGUI.EndChangeCheck())
                {
                    externalOutputNode.externalOutputDimension = (ExternalOutputNode.ExternalOutputDimension)outputDimension;
                    switch(outputDimension)
                    {
                        case ExternalOutputNode.ExternalOutputDimension.Texture2D:
                            externalOutputNode.rtSettings.dimension = OutputDimension.Texture2D;
                            break;
                        case ExternalOutputNode.ExternalOutputDimension.Texture3D:
                            externalOutputNode.rtSettings.dimension = OutputDimension.Texture3D;
                            break;
                    }
                    externalOutputNode.OnSettingsChanged();
                    MarkDirtyRepaint();
                }

                EditorGUI.BeginChangeCheck();
                var outputType = EditorGUILayout.EnumPopup("Output Type", externalOutputNode.external2DOoutputType);
                if(EditorGUI.EndChangeCheck())
                {
                    externalOutputNode.external2DOoutputType = (ExternalOutputNode.External2DOutputType)outputType;
                    MarkDirtyRepaint();
                }
                GUILayout.Space(8);
            }
            );
            nodeSettings.AddToClassList("MaterialInspector");

            controlsContainer.Add(nodeSettings);

            if(graph.isRealtime)
            {
                controlsContainer.Add(new IMGUIContainer(() =>
                {
                    EditorGUILayout.HelpBox("Using this node in a realtime Mixture Graph is not supported", MessageType.Warning);
                }));
            }
            else
            {
                // Add Buttons
                saveButton = new Button(SaveExternal)
                {
                    text = "Save As..."
                };
                updateButton = new Button(UpdateExternal)
                {
                    text = "Update"
                };

                var horizontal = new VisualElement();
                horizontal.style.flexDirection = FlexDirection.Row;
                horizontal.Add(saveButton);
                horizontal.Add(updateButton);
                controlsContainer.Add(horizontal);
                UpdateButtons();
            }
        }

        void UpdateButtons()
        {
            if(graph.isRealtime)
            {
                saveButton.style.display = DisplayStyle.None;
                updateButton.style.display = DisplayStyle.None;
            }
            else
            {
                var externalOutputNode = nodeTarget as ExternalOutputNode;
                // Manage First save or Update
                saveButton.style.display = DisplayStyle.Flex;
                updateButton.style.display = DisplayStyle.Flex;
                updateButton.SetEnabled(externalOutputNode.asset != null);
            }
        }

        void SaveExternal()
        {
            graph.SaveExternalTexture(nodeTarget as ExternalOutputNode, true);
            UpdateButtons();
        }

        void UpdateExternal()
        {
            graph.SaveExternalTexture(nodeTarget as ExternalOutputNode, false);
            UpdateButtons();
        }
    }
}