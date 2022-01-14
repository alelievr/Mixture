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
        Toggle          rgb, r, g, b, a, srgb;
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
            previewContainer = previewRoot.Q("PreviewContainer");
            rgb = previewRoot.Q("ToggleRGB") as Toggle;
            r = previewRoot.Q("ToggleR") as Toggle;
            g = previewRoot.Q("ToggleG") as Toggle;
            b = previewRoot.Q("ToggleB") as Toggle;
            a = previewRoot.Q("ToggleA") as Toggle;
            srgb = previewRoot.Q("ToggleSRGB") as Toggle;
            mipmapSlider = previewRoot.Q("MipMapSlider") as SliderInt;
            mipmapInputs = previewRoot.Q("MipMapInput") as VisualElement;
            currentMipIndex = previewRoot.Q("MipMapNumberText") as Label;
            sliceInputs = previewRoot.Q("SliceInputs");
            sliceSlider = previewRoot.Q("SliceSlider") as SliderInt;
            currentSliceIndex = previewRoot.Q("SliceNumber") as IntegerField;
            imageInfo = previewRoot.Q("ImageInfo");
            textureInfo = previewRoot.Q("ImageInfoText") as Label;
            collapseButton = previewRoot.Q("PreviewFoldout") as Button;
            previewImage = previewRoot.Q("PreviewImage");

            previewImage.style.width = 200;
            previewImage.style.height = 200;

            previewImage.Add(new IMGUIContainer(DrawPreviewImage));

            // TODO: determine image size rect to fit the node

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
            previewRect.position = Vector2.zero;

			switch (node.previewTexture.dimension)
			{
				case TextureDimension.Tex2D:
					MixtureUtils.texture2DPreviewMaterial.SetTexture("_MainTex", node.previewTexture);
					MixtureUtils.texture2DPreviewMaterial.SetVector("_Size", new Vector4(node.previewTexture.width,node.previewTexture.height, 1, 1));
					MixtureUtils.texture2DPreviewMaterial.SetVector("_Channels", MixtureEditorUtils.GetChannelsMask(node.previewMode));
					MixtureUtils.texture2DPreviewMaterial.SetFloat("_PreviewMip", node.previewMip);
					MixtureUtils.texture2DPreviewMaterial.SetFloat("_EV100", node.previewEV100);
                    MixtureUtils.texture2DPreviewMaterial.SetInt("_IsSRGB", node.previewSRGB ? 1 : 0);

					if (Event.current.type == EventType.Repaint)
						EditorGUI.DrawPreviewTexture(previewRect, node.previewTexture, MixtureUtils.texture2DPreviewMaterial, ScaleMode.ScaleToFit, 0, 0);
					break;
				case TextureDimension.Tex3D:
					MixtureUtils.texture3DPreviewMaterial.SetTexture("_Texture3D", node.previewTexture);
					MixtureUtils.texture3DPreviewMaterial.SetVector("_Channels", MixtureEditorUtils.GetChannelsMask(node.previewMode));
					MixtureUtils.texture3DPreviewMaterial.SetFloat("_PreviewMip", node.previewMip);
					MixtureUtils.texture3DPreviewMaterial.SetFloat("_Depth", (node.previewSlice + 0.5f) / node.settings.GetResolvedDepth(graphView.graph));
					MixtureUtils.texture3DPreviewMaterial.SetFloat("_EV100", node.previewEV100);

					if (Event.current.type == EventType.Repaint)
						EditorGUI.DrawPreviewTexture(previewRect, Texture2D.whiteTexture, MixtureUtils.texture3DPreviewMaterial, ScaleMode.ScaleToFit, 0, 0, ColorWriteMask.Red);
					break;
				case TextureDimension.Cube:
					MixtureUtils.textureCubePreviewMaterial.SetTexture("_Cubemap", node.previewTexture);
					MixtureUtils.textureCubePreviewMaterial.SetVector("_Channels", MixtureEditorUtils.GetChannelsMask(node.previewMode));
					MixtureUtils.textureCubePreviewMaterial.SetFloat("_PreviewMip", node.previewMip);
					MixtureUtils.textureCubePreviewMaterial.SetFloat("_EV100", node.previewEV100);

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
