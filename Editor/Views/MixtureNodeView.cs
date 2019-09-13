using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
	[NodeCustomEditor(typeof(MixtureNode))]
	public class MixtureNodeView : BaseNodeView
	{
		protected VisualElement propertyEditorUI;
        protected VisualElement previewContainer;

        protected new MixtureGraphView  owner => base.owner as MixtureGraphView;
		protected new MixtureNode       nodeTarget => base.nodeTarget as MixtureNode;

		Dictionary< Material, MaterialProperty[] >  oldMaterialProperties = new Dictionary<Material, MaterialProperty[]>();
		Dictionary< Material, MaterialEditor >      materialEditors = new Dictionary<Material, MaterialEditor>();

		protected virtual string header => string.Empty;

		protected virtual bool hasPreview => false;
		protected override bool hasSettings => nodeTarget.hasSettings;

		protected override VisualElement CreateSettingsView()
		{
			var view = new MixtureRTSettingsView(nodeTarget, owner);
            view.AddToClassList("RTSettingsView");
			view.RegisterChangedCallback(nodeTarget.OnSettingsChanged);

			return view;
		}

		const string stylesheetName = "MixtureCommon";

        public override void Enable()
		{
			var stylesheet = Resources.Load<StyleSheet>(stylesheetName);
			if(!styleSheets.Contains(stylesheet))
				styleSheets.Add(stylesheet);

			// When we change the output dimension, we want to update the output ports
			// TODO: there is probably a race condition here between the port that changes type
			// and the MixtureGraphView callback that run the processor
			owner.graph.onOutputTextureUpdated += UpdatePorts;
			nodeTarget.onSettingsChanged += UpdatePorts;
			nodeTarget.onSettingsChanged += () => owner.processor.Run();
			nodeTarget.onProcessed += UpdateTexturePreview;
			
			propertyEditorUI = new VisualElement();
			controlsContainer.Add(propertyEditorUI);

			// Fix the size of the node
			style.width = nodeTarget.nodeWidth;

			propertyEditorUI.AddToClassList("PropertyEditorUI");
			controlsContainer.AddToClassList("ControlsContainer");

			if (header != string.Empty)
			{
				var title = new Label(header);
				title.AddToClassList("PropertyEditorTitle");
				propertyEditorUI.Add(title);
			}

			if (nodeTarget.showDefaultInspector)
			{
				DrawDefaultInspector();
			}

			UpdateTexturePreview();

            propertyEditorUI.style.display = DisplayStyle.Flex;
        }

		~MixtureNodeView()
		{
			MixturePropertyDrawer.UnregisterGraph(owner.graph);
		}

		void UpdatePorts()
		{
			nodeTarget.UpdateAllPorts();
			RefreshPorts();
		}

		void UpdateTexturePreview()
		{
			if (hasPreview && previewContainer == null)
			{
                CreateTexturePreview(ref previewContainer, nodeTarget.previewTexture); // TODO : Add Slice Preview
                controlsContainer.Add(previewContainer);
			}
		}

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
		protected bool MaterialPropertiesGUI(Material material, bool autoLabelWidth = true)
		{
			if (material == null || material.shader == null)
				return false;
				
			if (autoLabelWidth)
			{
				EditorGUIUtility.wideMode = false;
				EditorGUIUtility.labelWidth = nodeTarget.nodeWidth / 3.0f;
			}

			MaterialProperty[] properties = MaterialEditor.GetMaterialProperties(new []{material});
			var portViews = GetPortViewsFromFieldName(nameof(ShaderNode.materialInputs));

			MaterialEditor  editor;
			if (!materialEditors.TryGetValue(material, out editor))
			{
				editor = materialEditors[material] = Editor.CreateEditor(material) as MaterialEditor;
				MixturePropertyDrawer.RegisterEditor(editor, this, owner.graph);
			}

			bool propertiesChanged = CheckPropertyChanged(material, properties);

			foreach (var property in properties)
			{
				if ((property.flags & (MaterialProperty.PropFlags.HideInInspector | MaterialProperty.PropFlags.PerRendererData)) != 0)
					continue;

				// Retrieve the port view from the property name
				var portView = portViews?.FirstOrDefault(p => p.portData.identifier == property.name);
				if (portView != null && portView.connected)
					continue;

				float h = editor.GetPropertyHeight(property, property.displayName);
				Rect r = EditorGUILayout.GetControlRect(true, h, EditorStyles.layerMaskField);

				editor.ShaderProperty(r, property, property.displayName);
			}

			return propertiesChanged;
		}

		protected void CreateTexturePreview(ref VisualElement previewContainer, Texture texture, int currentSlice = 0)
		{
			if (texture == null)
			{
				previewContainer = null;
				return;
			}

			if (previewContainer == null)
                previewContainer = new VisualElement();
			else
            	previewContainer.Clear();

			VisualElement texturePreview = new VisualElement();
			previewContainer.Add(texturePreview);

            switch (texture.dimension)
			{
				case TextureDimension.Tex2D:
					CreateTexture2DPreview(texturePreview, texture);
					break;
				case TextureDimension.Tex2DArray:
					CreateTexture2DArrayPreview(texturePreview, texture, currentSlice);
					break;
				case TextureDimension.Tex3D:
					CreateTexture3DPreview(texturePreview, texture, currentSlice);
					break;
				case TextureDimension.Cube:
					CreateTextureCubePreview(texturePreview, texture, currentSlice);
					break;
				default:
					Debug.LogError(texture + " is not a supported type for preview");
					return;
			}
			
			Button togglePreviewButton = null;
			togglePreviewButton = new Button(() => {
				m_PreviewVisible = !m_PreviewVisible;
				UpdatePreviewCollapseState();
			});
			togglePreviewButton.ClearClassList();
			togglePreviewButton.AddToClassList("PreviewToggleButton");
			previewContainer.Add(togglePreviewButton);

			UpdatePreviewCollapseState();

			void UpdatePreviewCollapseState()
			{
				if (m_PreviewVisible)
				{
					texturePreview.style.display = DisplayStyle.Flex;
					togglePreviewButton.RemoveFromClassList("Collapsed");
				}
				else
				{
					texturePreview.style.display = DisplayStyle.None;
					togglePreviewButton.AddToClassList("Collapsed");
				}
			}
        }


		Rect GetPreviewRect(Texture texture)
		{
			float width = Mathf.Min(nodeTarget.nodeWidth, texture.width);
			float height = Mathf.Min(nodeTarget.nodeWidth, texture.height);
			return GUILayoutUtility.GetRect(1, width, 1, height);
		}

        enum PreviewChannels
        {
            R       = 1,
            G       = 2,
            B       = 4,
            A       = 8,
            RG      = R|G,
            RB      = R|B,
            GB      = G|B,
            RGB     = R|G|B,
            RGBA    = R|G|B|A,
        }

        static Vector4 GetChannelsMask(PreviewChannels channels)
        {
            return new Vector4(
                (channels & PreviewChannels.R) == 0 ? 0 : 1,
                (channels & PreviewChannels.G) == 0 ? 0 : 1,
                (channels & PreviewChannels.B) == 0 ? 0 : 1,
                (channels & PreviewChannels.A) == 0 ? 0 : 1
                );
        }

        [SerializeField]
        PreviewChannels m_PreviewMode = PreviewChannels.RGBA;
        [SerializeField]
        float m_PreviewMip = 0.0f;
        [SerializeField]
        bool m_PreviewSRGB = true;
        [SerializeField]
        bool m_PreviewVisible = true;

		void CreateTexture2DPreview(VisualElement previewContainer, Texture texture)
		{
		        var previewElement = new IMGUIContainer(() => {
                    // square image:
                    using(new GUILayout.HorizontalScope(EditorStyles.toolbar, GUILayout.Height(12)))
                    {
                        EditorGUI.BeginChangeCheck();

                        bool r = GUILayout.Toggle( (m_PreviewMode & PreviewChannels.R) != 0,"R", EditorStyles.toolbarButton);
                        bool g = GUILayout.Toggle( (m_PreviewMode & PreviewChannels.G) != 0,"G", EditorStyles.toolbarButton);
                        bool b = GUILayout.Toggle( (m_PreviewMode & PreviewChannels.B) != 0,"B", EditorStyles.toolbarButton);
                        bool a = GUILayout.Toggle( (m_PreviewMode & PreviewChannels.A) != 0,"A", EditorStyles.toolbarButton);

                        if (EditorGUI.EndChangeCheck())
                        {
                            m_PreviewMode =
                            (r ? PreviewChannels.R : 0) |
                            (g ? PreviewChannels.G : 0) |
                            (b ? PreviewChannels.B : 0) |
                            (a ? PreviewChannels.A : 0);
                        }

                        GUILayout.Space(8);

                        m_PreviewMip = GUILayout.HorizontalSlider(m_PreviewMip, 0.0f, 5.0f, GUILayout.Width(64));
                        GUILayout.Label("Mip #"+m_PreviewMip.ToString("0"), EditorStyles.toolbarButton);

                        GUILayout.FlexibleSpace();
                    }

                    MixtureUtils.texture2DPreviewMaterial.SetTexture("_MainTex", texture);
                    MixtureUtils.texture2DPreviewMaterial.SetVector("_Size", new Vector4(texture.width,texture.height,1,1));
                    MixtureUtils.texture2DPreviewMaterial.SetVector("_Channels", GetChannelsMask(m_PreviewMode));
                    MixtureUtils.texture2DPreviewMaterial.SetFloat("_PreviewMip", m_PreviewMip);
                    EditorGUI.DrawPreviewTexture(GetPreviewRect(texture), texture, MixtureUtils.texture2DPreviewMaterial, ScaleMode.ScaleToFit, 0, 0);
                 
                });
			previewContainer.Add(previewElement);
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
				MixtureUtils.textureArrayPreviewMaterial.SetTexture("_TextureArray", texture);
				MixtureUtils.textureArrayPreviewMaterial.SetFloat("_Slice", currentSlice);
				EditorGUI.DrawPreviewTexture(GetPreviewRect(texture), Texture2D.whiteTexture, MixtureUtils.textureArrayPreviewMaterial, ScaleMode.ScaleToFit, 0, 0);
            });
			previewSliceIndex.RegisterValueChangedCallback((ChangeEvent< int > a) => {
				currentSlice = a.newValue;
			});
			previewContainer.Add(previewSliceIndex);
			previewContainer.Add(previewImageSlice);
		}
		
		void CreateTexture3DPreview(VisualElement previewContainer, Texture texture, int currentSlice)
		{
			// TODO: 3D Texture preview material with ray-marching
			var previewSliceIndex = new SliderInt(0, TextureUtils.GetSliceCount(texture) - 1)
			{
				label = "Slice",
				value = currentSlice,
			};
			var previewImageSlice = new IMGUIContainer(() => {
				// square image:
				MixtureUtils.texture3DPreviewMaterial.SetTexture("_Texture3D", texture);
				MixtureUtils.texture3DPreviewMaterial.SetFloat("_Depth", ((float)currentSlice + 0.5f) / nodeTarget.rtSettings.GetDepth(owner.graph));
				EditorGUI.DrawPreviewTexture(GetPreviewRect(texture), Texture2D.whiteTexture, MixtureUtils.texture3DPreviewMaterial, ScaleMode.ScaleToFit, 0, 0);
            });
			previewSliceIndex.RegisterValueChangedCallback((ChangeEvent< int > a) => {
				currentSlice = a.newValue;
			});
			previewContainer.Add(previewSliceIndex);
			previewContainer.Add(previewImageSlice);
		}

		void CreateTextureCubePreview(VisualElement previewContainer, Texture texture, int currentSlice)
		{
			var previewImageSlice = new IMGUIContainer(() => {
				// square image:
				MixtureUtils.textureCubePreviewMaterial.SetTexture("_Cubemap", texture);
				MixtureUtils.textureCubePreviewMaterial.SetFloat("_Slice", currentSlice);
				EditorGUI.DrawPreviewTexture(GetPreviewRect(texture), Texture2D.whiteTexture, MixtureUtils.textureCubePreviewMaterial, ScaleMode.ScaleToFit, 0, 0);
            });
			previewContainer.Add(previewImageSlice);
		}
	}
}