using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using GraphProcessor;
using System.Collections.Generic;
using UnityEngine.Rendering;
using Unity.Collections;
using System;
using System.Linq;
using TextureCompressionQuality = UnityEngine.TextureCompressionQuality;
using UnityEngine.Experimental.Rendering;

namespace Mixture
{
    [NodeCustomEditor(typeof(ExternalOutputNode))]
    public class ExternalOutputNodeView : OutputNodeView
    {
        Button saveButton;
        Button updateButton;

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
                var outputType = EditorGUILayout.EnumPopup("Output Type", externalOutputNode.outputType);
                if(EditorGUI.EndChangeCheck())
                {
                    externalOutputNode.outputType = (ExternalOutputNode.OutputType)outputType;
                    MarkDirtyRepaint();
                }
                GUILayout.Space(8);
            }
            );
            nodeSettings.AddToClassList("MaterialInspector");

            controlsContainer.Add(nodeSettings);

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