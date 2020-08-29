using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Linq;
using System.Reflection;

namespace Mixture
{
    // Keep the code around but it's not used. I'll enable it when the API for rendering
    // with a material in UIElement is usable
	public class NodeTexturePreview : VisualElement
    {
        MixtureNodeView nodeView;

        VisualElement previewRoot;
        
        // Texture Preview elements
        VisualElement   previewContainer;
        Toggle          r, g, b, a;
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
            previewRoot = Resources.Load<VisualTreeAsset>("UI Blocks/Preview").CloneTree();
            Add(previewRoot);

            // Load additional resources:
            arrowUp = Resources.Load<Texture2D>("Collapse-Down");
            arrowDown = Resources.Load<Texture2D>("Collapse-Up");

            // Init all preview components:
            previewContainer = this.Q("PreviewContainer");
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

            previewImage.generateVisualContent += DrawPreview;

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

        static Vertex[] rectangleVertices = new Vertex[]
        {
            new Vertex{ position = new Vector3(0, 0), uv = new Vector2(0, 0), tint = Color.red},
            new Vertex{ position = new Vector3(0, 1), uv = new Vector2(0, 1), tint = Color.red},
            new Vertex{ position = new Vector3(1, 1), uv = new Vector2(1, 1), tint = Color.red},
            new Vertex{ position = new Vector3(1, 0), uv = new Vector2(1, 0), tint = Color.red},
        };

        static ushort[] rectangleIndices = new ushort[]
        {
            0, 1, 2, 2, 3, 0
        };

        void DrawPreview(MeshGenerationContext mgc)
        {
            // Why do we need to re-allocate everything every time we render???
            var allocWithMaterial = mgc.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).FirstOrDefault(m => m.Name == "Allocate" && m.GetParameters().Length == 5);
            var rectangle = allocWithMaterial.Invoke(mgc, new object[]{
                4, 6, null, null, 0
            }) as MeshWriteData;

            foreach (var vertex in rectangleVertices)
            {
                var v = vertex;

                // Change the size of the thing
                v.position *= 100; // TODO
                rectangle.SetNextVertex(v);
            }
            rectangle.SetAllIndices(rectangleIndices);
        }
    }
}
