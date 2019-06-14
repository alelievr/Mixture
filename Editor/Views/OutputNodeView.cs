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
using Unity.Collections;

namespace Mixture
{
    [NodeCustomEditor(typeof(OutputNode))]
    public class OutputNodeView : MixtureNodeView
    {
        VisualElement	shaderCreationUI;
        VisualElement	materialEditorUI;

        VisualElement   previewContainer;
        MaterialEditor	materialEditor;
        OutputNode		outputNode;
        MixtureGraph    graph;

        // Materials used to draw the UI:
        Material        drawTextureArraySliceMaterial;
        // Texture3D materials

        static readonly Vector2 nodeViewSize = new Vector2(330, 400);

        public override void Enable()
        {
            outputNode = nodeTarget as OutputNode;
            graph = owner.graph as MixtureGraph;
            outputNode.onTempRenderTextureUpdated += UpdatePreviewImage;

            // Fix the size of the node
            var currentPos = GetPosition();
            SetPosition(new Rect(currentPos.x, currentPos.y, nodeViewSize.x, nodeViewSize.y));

            graph.onOutputTextureUpdated += UpdatePreviewImage;

            drawTextureArraySliceMaterial = new Material(Shader.Find("Hidden/MixtureTextureArrayPreview"));

            AddControls();
        }

        void AddControls()
        {
            var targetSizeField = FieldFactory.CreateField(typeof(Vector2Int), outputNode.targetSize, (newValue) => {
                Vector2Int v = (Vector2Int)newValue;
                v.x = Mathf.Clamp(v.x, 2, 32768);
                v.y = Mathf.Clamp(v.y, 2, 32768);
                owner.RegisterCompleteObjectUndo("Updated Target Size " + newValue);
                outputNode.targetSize = v;
                graph.UpdateOutputTexture();
            });
            (targetSizeField as Vector2IntField).label = "Final Size";

            var sliceCountField = FieldFactory.CreateField(typeof(int), outputNode.sliceCount, (newValue) => {
                int v = Mathf.Clamp((int)newValue, 1, 128);
                owner.RegisterCompleteObjectUndo("Updated Slice Count " + newValue);
                outputNode.sliceCount = v;
                graph.UpdateOutputTexture();
            });
            (sliceCountField as IntegerField).label = "Slice Count";

            var graphicsFormatField = new EnumField(outputNode.format) {
                label = "Format",
            };
            graphicsFormatField.RegisterValueChangedCallback(e => {
                owner.RegisterCompleteObjectUndo("Updated Graphics Format " + e.newValue);
                outputNode.format = (GraphicsFormat)e.newValue;
                graph.UpdateOutputTexture();
            });
            var textureDimensionField = new EnumField(outputNode.dimension) {
                label = "Dimension"
            };
            textureDimensionField.RegisterValueChangedCallback(e => {
                if (outputNode.dimension == (TextureDimension)e.newValue)
                    return ;
                owner.RegisterCompleteObjectUndo("Updated Texture Dimension" + e.newValue);
                outputNode.dimension = (TextureDimension)e.newValue;
                graph.UpdateOutputTexture();
            });
            var filterModeField = new EnumField(outputNode.filterMode) {
                label = "Filter Mode",
            };
            filterModeField.RegisterValueChangedCallback(e => {
                owner.RegisterCompleteObjectUndo("Updated Graphics Format " + e.newValue);
                outputNode.filterMode = (FilterMode)e.newValue;
                graph.UpdateOutputTexture();
            });

            controlsContainer.Add(targetSizeField);
            controlsContainer.Add(sliceCountField);
            controlsContainer.Add(graphicsFormatField);
            controlsContainer.Add(textureDimensionField);
            controlsContainer.Add(filterModeField);

            previewContainer = new VisualElement();
            controlsContainer.Add(previewContainer);
            UpdatePreviewImage();

            // Enforce the image size so we don't have a giant preview
            controlsContainer.style.width = nodeViewSize.x;
            controlsContainer.style.height = nodeViewSize.y;

            controlsContainer.Add(new Button(SaveTexture) {
                text = "Save"
            });
        }

        void UpdatePreviewImage()
        {
            previewContainer.Clear();

            if (outputNode.tempRenderTexture == null)
                return;

            switch (graph.outputTexture)
            {
                case Texture2D t:
                    var previewImage = new Image
                    {
                        image = outputNode.tempRenderTexture,
                        scaleMode = ScaleMode.StretchToFill,
                    };
                    previewContainer.Add(previewImage);
                    break;
                case Texture2DArray t:
                    var previewSliceIndex = new SliderInt(0, outputNode.tempRenderTexture.volumeDepth)
                    {
                        label = "Slice",
                        value = outputNode.currentSlice,
                    };
                    var previewImageSlice = new IMGUIContainer(() => {
                        var rect = EditorGUILayout.GetControlRect(GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
                        drawTextureArraySliceMaterial.SetTexture("_TextureArray", outputNode.tempRenderTexture);
                        drawTextureArraySliceMaterial.SetFloat("_Slice", outputNode.currentSlice);
                        EditorGUI.DrawPreviewTexture(rect, Texture2D.whiteTexture, drawTextureArraySliceMaterial);
                    });
                    previewSliceIndex.RegisterValueChangedCallback((ChangeEvent< int > a) => {
                        outputNode.currentSlice = a.newValue;
                    });
                    previewContainer.Add(previewSliceIndex);
                    previewContainer.Add(previewImageSlice);
                    break;
                // TODO: Texture2DArray and Texture3D
                default:
                    Debug.LogError(graph.outputTexture + " is not a supported type for preview");
                    return;
            }
        }

        // Write the rendertexture value to the graph main texture asset
        void SaveTexture()
        {
            // Retrieve the texture from the GPU:
            var src = outputNode.tempRenderTexture;
            Debug.Log("src: " + src.dimension);
            var request = AsyncGPUReadback.Request(src, 0, 0, src.width, 0, src.height, 0, src.volumeDepth, (r) => {
                WriteRequestResult(r, graph.outputTexture);
            });

            request.WaitForCompletion();
        }

        void WriteRequestResult(AsyncGPUReadbackRequest request, Texture output)
        {
            NativeArray< Color32 >    colors;

            if (request.hasError)
            {
                Debug.LogError("Can't readback the texture from GPU");
                return ;
            }

            switch (graph.outputTexture)
            {
                case Texture2D t:
                    colors = request.GetData< Color32 >(0);
                    t.SetPixels32(colors.ToArray());
                    t.Apply();
                    break;
                case Texture2DArray t:
                    for (int i = 0; i < outputNode.tempRenderTexture.volumeDepth; i++)
                    {
                        colors = request.GetData< Color32 >(i);
                        t.SetPixels32(colors.ToArray(), i);
                    }
                    t.Apply();
                    break;
                case Texture3D t:
                    for (int i = 0; i < outputNode.tempRenderTexture.volumeDepth; i++)
                    {
                        colors = request.GetData< Color32 >(i);
                        t.SetPixels32(colors.ToArray(), i);
                    }
                    t.Apply();
                    break;
                default:
                    Debug.LogError(graph.outputTexture + " is not a supported type for saving");
                    return ;
            }
        }
    }
}