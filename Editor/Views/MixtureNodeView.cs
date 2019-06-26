using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
	public class MixtureNodeView : BaseNodeView
	{
        protected new MixtureGraphView  owner => base.owner as MixtureGraphView;
        protected new MixtureNode       nodeTarget => base.nodeTarget as MixtureNode;

        Dictionary< Material, MaterialProperty[] >  oldMaterialProperties = new Dictionary<Material, MaterialProperty[]>();
        Dictionary< Material, MaterialEditor >      materialEditors = new Dictionary<Material, MaterialEditor>();

		public override void Enable() => Debug.Log("New Node View: " + this);

		bool CheckPropertyChanged(Material material, MaterialProperty[] properties)
		{
			bool propertyChanged = false;
            MaterialProperty[]  oldProperties;
            oldMaterialProperties.TryGetValue(material, out oldProperties);

			if (oldProperties != null)
			{
				// Check if shader was changed (new/deleted properties)
				if (properties.Length != oldProperties.Length)
				{
					propertyChanged = true;
				}
				else
				{
					for (int i = 0; i < properties.Length; i++)
					{
						if (properties[i].type != oldProperties[i].type)
							propertyChanged = true;
						if (properties[i].displayName != oldProperties[i].displayName)
							propertyChanged = true;
						if (properties[i].flags != oldProperties[i].flags)
							propertyChanged = true;
						if (properties[i].name != oldProperties[i].name)
							propertyChanged = true;
					}
				}
			}

            oldMaterialProperties[material] = MaterialEditor.GetMaterialProperties(new []{material});

            return propertyChanged;
		}

        // Custom property draw, we don't want things that are connected to an edge or useless like the render queue
		protected bool MaterialPropertiesGUI(Material material)
		{
			if (material == null || material.shader == null)
				return false;

            MaterialProperty[] properties = MaterialEditor.GetMaterialProperties(new []{material});
			var portViews = GetPortViewsFromFieldName(nameof(ShaderNode.materialInputs));

            MaterialEditor  editor;
            if (!materialEditors.TryGetValue(material, out editor))
                editor = materialEditors[material] = Editor.CreateEditor(material) as MaterialEditor;

			bool propertiesChanged = CheckPropertyChanged(material, properties);

			foreach (var property in properties)
			{
				if ((property.flags & (MaterialProperty.PropFlags.HideInInspector | MaterialProperty.PropFlags.PerRendererData)) != 0)
					continue;

				// Retrieve the port view from the property name
				var portView = portViews.FirstOrDefault(p => p.portData.identifier == property.name);
				if (portView == null || portView.connected)
					continue;

				float h = editor.GetPropertyHeight(property, property.displayName);
				Rect r = EditorGUILayout.GetControlRect(true, h, EditorStyles.layerMaskField);

				editor.ShaderProperty(r, property, property.displayName);
			}

            return propertiesChanged;
		}

		protected void CreateTexturePreview(VisualElement previewContainer, Texture texture, int currentSlice = 0)
		{
            previewContainer.Clear();

            if (texture == null)
                return;

            switch (texture.dimension)
            {
                case TextureDimension.Tex2D:
					CreateTexture2DPreview(previewContainer, texture);
                    break;
                case TextureDimension.Tex2DArray:
					CreateTexture2DArrayPreview(previewContainer, texture, currentSlice);
                    break;
                // TODO: Texture3D
                default:
                    Debug.LogError(texture + " is not a supported type for preview");
                    return;
            }
		}

		void CreateTexture2DPreview(VisualElement previewContainer, Texture texture)
		{
			var previewImage = new Image
			{
				image = texture,
				scaleMode = ScaleMode.StretchToFill,
			};
			previewContainer.Add(previewImage);
		}

		void CreateTexture2DArrayPreview(VisualElement previewContainer, Texture texture, int currentSlice)
		{
			var previewSliceIndex = new SliderInt(0, TextureUtils.GetSliceCount(texture) - 1)
			{
				label = "Slice",
				value = currentSlice,
			};
			var previewImageSlice = new IMGUIContainer(() => {
				// square image:
				int size = (int)previewContainer.parent.style.width.value.value;
				var rect = EditorGUILayout.GetControlRect(GUILayout.Height(size), GUILayout.Width(size));
				MixtureUtils.textureArrayPreviewMaterial.SetTexture("_TextureArray", texture);
				MixtureUtils.textureArrayPreviewMaterial.SetFloat("_Slice", currentSlice);
				EditorGUI.DrawPreviewTexture(rect, Texture2D.whiteTexture, MixtureUtils.textureArrayPreviewMaterial);
			});
			previewSliceIndex.RegisterValueChangedCallback((ChangeEvent< int > a) => {
				currentSlice = a.newValue;
			});
			previewContainer.Add(previewSliceIndex);
			previewContainer.Add(previewImageSlice);
		}
	}
}