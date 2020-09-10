using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine.Rendering;

namespace Mixture
{
    // Keep the code around but it's not used. I'll enable it when the API for rendering
    // with a material in UIElement is usable
	public class NodeTexturePreview : VisualElement
    {
        MixtureNodeView     nodeView;
        MixtureGraphView    graphView;

        VisualElement previewRoot;
        
        // Texture Preview elements
        VisualElement   previewContainer;
        Toggle          rgb, r, g, b, a;
        VisualElement   mipmapInputs;
        SliderInt       mipmapSlider;
        Label           currentMipIndex;
        VisualElement   sliceInputs;
        SliderInt       sliceSlider;
        IntegerField    currentSliceIndex;
        VisualElement   imageInfo;
        Label           textureInfo;
        Button          collapseButton;
        VisualElement   previewImage;

        Texture2D       arrowUp;
        Texture2D       arrowDown;

        public NodeTexturePreview(MixtureNodeView view)
        {
            nodeView = view;
            graphView = nodeView.owner as MixtureGraphView;
            previewRoot = Resources.Load<VisualTreeAsset>("UI Blocks/Preview").CloneTree();
            Add(previewRoot);

            // Load additional resources:
            arrowUp = Resources.Load<Texture2D>("Collapse-Down");
            arrowDown = Resources.Load<Texture2D>("Collapse-Up");

            // Init all preview components:
            previewContainer = this.Q("PreviewContainer");
            rgb = this.Q("ToggleRGB") as Toggle;
            r = this.Q("ToggleR") as Toggle;
            g = this.Q("ToggleG") as Toggle;
            b = this.Q("ToggleB") as Toggle;
            a = this.Q("ToggleA") as Toggle;

            mipmapSlider = this.Q("MipMapSlider") as SliderInt;
            mipmapInputs = this.Q("MipMapInput") as VisualElement;
            currentMipIndex = this.Q("MipMapNumberText") as Label;
            sliceInputs = this.Q("SliceInputs");
            sliceSlider = this.Q("SliceSlider") as SliderInt;
            currentSliceIndex = this.Q("SliceNumber") as IntegerField;
            imageInfo = this.Q("ImageInfo");
            textureInfo = this.Q("ImageInfoText") as Label;
            collapseButton = this.Q("PreviewFoldout") as Button;
            previewImage = this.Q("PreviewImage");

            previewImage.Add(new IMGUIContainer(DrawPreviewImage));

            collapseButton.clicked += PreviewColapse;

            // TODO: all events, preview shader ect...
        }

        void PreviewColapse()
        {
            if (previewContainer.style.display == DisplayStyle.None)
            {
                previewContainer.style.display = DisplayStyle.Flex;
                collapseButton.style.backgroundImage = arrowDown;
            }
            else
            {
                previewContainer.style.display = DisplayStyle.None;
                collapseButton.style.backgroundImage = arrowUp;
            }
        }

        void DrawPreviewImage()
        {
            var node = nodeView.nodeTarget as MixtureNode;
            var previewRect = previewImage.layout;

			switch (node.previewTexture.dimension)
			{
				case TextureDimension.Tex2D:
					MixtureUtils.texture2DPreviewMaterial.SetTexture("_MainTex", node.previewTexture);
					MixtureUtils.texture2DPreviewMaterial.SetVector("_Size", new Vector4(node.previewTexture.width,node.previewTexture.height, 1, 1));
					MixtureUtils.texture2DPreviewMaterial.SetVector("_Channels", MixtureEditorUtils.GetChannelsMask(node.previewMode));
					MixtureUtils.texture2DPreviewMaterial.SetFloat("_PreviewMip", node.previewMip);
					MixtureUtils.texture2DPreviewMaterial.SetFloat("_EV100", node.previewEV100);
					MixtureUtils.SetupIsSRGB(MixtureUtils.texture2DPreviewMaterial, node, graphView.graph);

					if (Event.current.type == EventType.Repaint)
						EditorGUI.DrawPreviewTexture(previewRect, node.previewTexture, MixtureUtils.texture2DPreviewMaterial, ScaleMode.ScaleToFit, 0, 0);
					break;
				case TextureDimension.Tex3D:
					MixtureUtils.texture3DPreviewMaterial.SetTexture("_Texture3D", node.previewTexture);
					MixtureUtils.texture3DPreviewMaterial.SetVector("_Channels", MixtureEditorUtils.GetChannelsMask(node.previewMode));
					MixtureUtils.texture3DPreviewMaterial.SetFloat("_PreviewMip", node.previewMip);
					MixtureUtils.texture3DPreviewMaterial.SetFloat("_Depth", (node.previewSlice + 0.5f) / node.rtSettings.GetDepth(graphView.graph));
					MixtureUtils.texture3DPreviewMaterial.SetFloat("_EV100", node.previewEV100);
					MixtureUtils.SetupIsSRGB(MixtureUtils.texture3DPreviewMaterial, node, graphView.graph);

					if (Event.current.type == EventType.Repaint)
						EditorGUI.DrawPreviewTexture(previewRect, Texture2D.whiteTexture, MixtureUtils.texture3DPreviewMaterial, ScaleMode.ScaleToFit, 0, 0, ColorWriteMask.Red);
					break;
				case TextureDimension.Cube:
					MixtureUtils.textureCubePreviewMaterial.SetTexture("_Cubemap", node.previewTexture);
					MixtureUtils.textureCubePreviewMaterial.SetVector("_Channels", MixtureEditorUtils.GetChannelsMask(node.previewMode));
					MixtureUtils.textureCubePreviewMaterial.SetFloat("_PreviewMip", node.previewMip);
					MixtureUtils.textureCubePreviewMaterial.SetFloat("_EV100", node.previewEV100);
					MixtureUtils.SetupIsSRGB(MixtureUtils.textureCubePreviewMaterial, node, graphView.graph);

					if (Event.current.type == EventType.Repaint)
						EditorGUI.DrawPreviewTexture(previewRect, Texture2D.whiteTexture, MixtureUtils.textureCubePreviewMaterial, ScaleMode.ScaleToFit, 0, 0);
					break;
				default:
					Debug.LogError(node.previewTexture + " is not a supported type for preview");
					break;
			}
        }
    }
}
