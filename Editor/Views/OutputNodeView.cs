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

        static readonly Vector2 nodeViewSize = new Vector2(330, 480);

        public override void Enable()
        {
            base.Enable();

            outputNode = nodeTarget as OutputNode;
            graph = owner.graph as MixtureGraph;
            outputNode.onTempRenderTextureUpdated += UpdatePreviewImage;

            // Fix the size of the node
            var currentPos = GetPosition();
            SetPosition(new Rect(currentPos.x, currentPos.y, nodeViewSize.x, nodeViewSize.y));

            graph.onOutputTextureUpdated += UpdatePreviewImage;

            controlsContainer.Add(new Button(SaveTexture) {
                text = "Save"
            });
        }

        void UpdatePreviewImage()
        {
            CreateTexturePreview(previewContainer, outputNode.tempRenderTexture, outputNode.currentSlice);
        }

        // Write the rendertexture value to the graph main texture asset
        void SaveTexture()
        {
            // Retrieve the texture from the GPU:
            var src = outputNode.tempRenderTexture;
            var request = AsyncGPUReadback.Request(src, 0, 0, src.width, 0, src.height, 0, src.volumeDepth, (r) => {
                WriteRequestResult(r, graph.outputTexture);
            });

            request.Update();

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

            switch (output)
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
                    Debug.LogError(output + " is not a supported type for saving");
                    return ;
            }

            EditorGUIUtility.PingObject(output);
        }
    }
}