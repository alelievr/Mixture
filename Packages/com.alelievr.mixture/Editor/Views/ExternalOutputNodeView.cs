using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using GraphProcessor;
using UnityEditor.Experimental.GraphView;
using System.Collections.Generic;
using System.Linq;

namespace Mixture
{
    [NodeCustomEditor(typeof(ExternalOutputNode))]
    public class ExternalOutputNodeView : OutputNodeView
    {
        List<(Button save, Button update)> buttons = new List<(Button, Button)>();

		public override void Enable(bool fromInspector)
        {
            var stylesheet = Resources.Load<StyleSheet>("ExternalOutputNodeView");

			if(styleSheets != null && !styleSheets.Contains(stylesheet))
				styleSheets.Add(stylesheet);

            base.Enable(fromInspector);

            // We can delete external outputs
            capabilities |= Capabilities.Deletable | Capabilities.Renamable;
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
                    EditorGUILayout.ObjectField("Asset", externalOutputNode.asset, typeof(Texture), false);
                    EditorGUI.EndDisabledGroup();
                }
                EditorGUI.BeginChangeCheck();
                var outputDimension = EditorGUILayout.EnumPopup("Dimension", externalOutputNode.externalOutputDimension);
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
                        case ExternalOutputNode.ExternalOutputDimension.Cubemap:
                            externalOutputNode.rtSettings.dimension = OutputDimension.CubeMap;
                            break;
                    }
                    externalOutputNode.OnSettingsChanged();
                    MarkDirtyRepaint();
                }

                if (externalOutputNode.externalOutputDimension == ExternalOutputNode.ExternalOutputDimension.Texture2D)
                {
                    EditorGUI.BeginChangeCheck();
                    var outputType = EditorGUILayout.EnumPopup("Type", externalOutputNode.external2DOoutputType);
                    if(EditorGUI.EndChangeCheck())
                    {
                        externalOutputNode.external2DOoutputType = (ExternalOutputNode.External2DOutputType)outputType;
                        MarkDirtyRepaint();
                    }
                }
                else if (externalOutputNode.externalOutputDimension == ExternalOutputNode.ExternalOutputDimension.Texture3D)
                {
                    EditorGUI.BeginChangeCheck();
                    var format = EditorGUILayout.EnumPopup("Format", externalOutputNode.external3DFormat);
                    if (EditorGUI.EndChangeCheck())
                    {
                        externalOutputNode.external3DFormat = (ConversionFormat)format;
                        MarkDirtyRepaint();
                    }
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
                var saveButton = new Button(SaveExternal)
                {
                    text = "Save As..."
                };
                var updateButton = new Button(UpdateExternal)
                {
                    text = "Update"
                };

                buttons.Add((saveButton, updateButton));

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
            foreach (var button in buttons)
            {
                if (button.save == null || button.update == null)
                    continue;

                if (graph.isRealtime)
                {
                    button.save.style.display = DisplayStyle.None;
                    button.update.style.display = DisplayStyle.None;
                }
                else
                {
                    var externalOutputNode = nodeTarget as ExternalOutputNode;
                    // Manage First save or Update
                    button.save.style.display = DisplayStyle.Flex;
                    button.update.style.display = DisplayStyle.Flex;
                    button.update.SetEnabled(externalOutputNode.asset != null);
                }
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

		public override void OnRemoved()
        {
            base.OnRemoved();

            // Manually disconnect the edges because we have a custom port handling in output nodes
            var inputPort = inputPortElements[outputNode.mainOutput.name];
            foreach (var i in inputPort.portView.GetEdges().ToList())
                owner.DisconnectView(i);
        }
    }
}