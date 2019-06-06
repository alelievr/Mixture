using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using GraphProcessor;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace Mixture
{
    [NodeCustomEditor(typeof(OutputNode))]
    public class OutputNodeView : MixtureNodeView
    {
        VisualElement	shaderCreationUI;
        VisualElement	materialEditorUI;

        Image           previewImage;
        MaterialEditor	materialEditor;
        OutputNode		outputNode;
        MixtureGraph    graph;

        static readonly Vector2 nodeViewSize = new Vector2(330, 400);

        public override void Enable()
        {
            outputNode = nodeTarget as OutputNode;
            graph = owner.graph as MixtureGraph;
            outputNode.onTempRenderTextureReferenceUpdated += UpdatePreviewImage;

            // Fix the size of the node
            var currentPos = GetPosition();
            SetPosition(new Rect(currentPos.x, currentPos.y, nodeViewSize.x, nodeViewSize.y));

            AddControls();
        }

        void AddControls()
        {
            var targetSizeField = FieldFactory.CreateField(typeof(Vector3Int), outputNode.targetSize, (newValue) => {
                owner.RegisterCompleteObjectUndo("Updated " + newValue);
                outputNode.targetSize = (Vector3Int)newValue;
            });
            (targetSizeField as Vector3IntField).label = "Final size";

            var graphicsFormatField = new EnumField(outputNode.format) {
                label = "format",
            };
            graphicsFormatField.RegisterValueChangedCallback(e => {
                owner.RegisterCompleteObjectUndo("Updated " + e.newValue);
                outputNode.format = (GraphicsFormat)e.newValue;
            });
            var textureDimensionField = new EnumField(outputNode.dimension) {
                label = "Dimension"
            };
            textureDimensionField.RegisterValueChangedCallback(e => {
                if (outputNode.dimension == (TextureDimension)e.newValue)
                    return ;
                owner.RegisterCompleteObjectUndo("Updated " + e.newValue);
                outputNode.dimension = (TextureDimension)e.newValue;
                graph.UpdateOutputTexture();
            });

            controlsContainer.Add(targetSizeField);
            controlsContainer.Add(graphicsFormatField);
            controlsContainer.Add(textureDimensionField);

            AddImagePreview();

            // Enforce the image size so we don't have a giant preview
            controlsContainer.style.width = nodeViewSize.x;
            controlsContainer.style.height = nodeViewSize.y;

            controlsContainer.Add(new Button(SaveTexture) {
                text = "Save"
            });
        }

        void AddImagePreview()
        {
            switch (graph.outputTexture)
            {
                case Texture2D t:
                    previewImage = new Image {
                        image = outputNode.tempRenderTexture,
                        scaleMode = ScaleMode.StretchToFill,
                    };
                    break;
                // TODO: Texture2DArray and Texture3D
                default:
                    Debug.LogError(graph.outputTexture + " is not a supported type for preview");
                    return ;
            }

            controlsContainer.Add(previewImage);
        }

        void UpdatePreviewImage()
        {
            if (previewImage == null)
                return;

            previewImage.image = outputNode.tempRenderTexture;
        }

        // Write the rendertexture value to the graph main texture asset
        void SaveTexture()
        {
            // TODO: GPU Async readback for this

            switch (graph.outputTexture)
            {
                case Texture2D t:
                    RenderTexture.active = outputNode.tempRenderTexture;
                    t.ReadPixels(new Rect(0, 0, outputNode.tempRenderTexture.width, outputNode.tempRenderTexture.height), 0, 0);
                    t.Apply();
                    break;
                // TODO: Texture2DArray and Texture3D
                default:
                    Debug.LogError(graph.outputTexture + " is not a supported type for saving");
                    return ;
            }
        }
    }
}