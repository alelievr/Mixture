using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
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
        protected VisualElement previewContainer;

        protected new MixtureGraphView  owner => base.owner as MixtureGraphView;
		protected new MixtureNode       nodeTarget => base.nodeTarget as MixtureNode;

		Dictionary< Material, MaterialProperty[] >  oldMaterialProperties = new Dictionary<Material, MaterialProperty[]>();
		Dictionary< Material, MaterialEditor >      materialEditors = new Dictionary<Material, MaterialEditor>();

		protected virtual string header => string.Empty;
		protected override bool hasSettings => nodeTarget.hasSettings;

		protected MixtureRTSettingsView settingsView;
	
		Label processTimeLabel;
		Image pinIcon;
		
		const string stylesheetName = "MixtureCommon";

		protected override VisualElement CreateSettingsView()
		{
			settingsView = new MixtureRTSettingsView(nodeTarget, owner);
            settingsView.AddToClassList("RTSettingsView");
            settingsView.RegisterChangedCallback(nodeTarget.OnSettingsChanged);

            return settingsView;
		}

        public override void Enable(bool fromInspector)
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

			// Fix the size of the node
			style.width = nodeTarget.nodeWidth;

			controlsContainer.AddToClassList("ControlsContainer");

			if (header != string.Empty)
			{
				var title = new Label(header);
				title.AddToClassList("PropertyEditorTitle");
				controlsContainer.Add(title);
			}

			// No preview in the inspector, we display it in the preview
			if (!fromInspector)
			{
				pinIcon = new Image{ image = MixtureEditorUtils.pinIcon, scaleMode = ScaleMode.ScaleToFit };
				var pinButton = new Button(() => {
					if (nodeTarget.isPinned)
						UnpinView();
					else
						PinView();
				});
				pinButton.Add(pinIcon);
				if (nodeTarget.isPinned)
					PinView();

				pinButton.AddToClassList("PinButton");
				rightTitleContainer.Add(pinButton);

				previewContainer = new VisualElement();
				previewContainer.AddToClassList("Preview");
				controlsContainer.Add(previewContainer);
				UpdateTexturePreview();
			}

			InitProcessingTimeLabel();

			if (nodeTarget.showDefaultInspector)
				DrawDefaultInspector(fromInspector);
        }

		~MixtureNodeView()
		{
			MixturePropertyDrawer.UnregisterGraph(owner.graph);
			owner.mixtureNodeInspector.RemovePinnedView(this);
		}

		void UpdatePorts()
		{
			nodeTarget.UpdateAllPorts();
			RefreshPorts();
		}

		void UpdateTexturePreview()
		{
			if (nodeTarget.hasPreview)
			{
                if (previewContainer.childCount == 0 || CheckDimensionChanged())
                    CreateTexturePreview(previewContainer, nodeTarget); // TODO : Add Slice Preview
            }
		}

        bool CheckDimensionChanged()
        {
            if(nodeTarget.previewTexture is CustomRenderTexture)
            {
                return (nodeTarget.previewTexture as CustomRenderTexture).dimension.ToString() != previewContainer.name;
            }
            else if (nodeTarget.previewTexture is Texture2D && previewContainer.name == "Texture2D")
                return true;
            else if (nodeTarget.previewTexture is Texture2DArray && previewContainer.name == "Texture2DArray")
                return true;
            else if (nodeTarget.previewTexture is Texture3D && previewContainer.name == "Texture3D")
                return true;
            else if (nodeTarget.previewTexture is Cubemap && previewContainer.name == "Cubemap")
                return true;
            else
                return false;
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
		protected bool MaterialPropertiesGUI(Material material, bool fromInspector, bool autoLabelWidth = true)
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

				int idx = material.shader.FindPropertyIndex(property.name);
				if (!fromInspector && material.shader.GetPropertyAttributes(idx).Contains("ShowInInspector"))
					continue;

				// Retrieve the port view from the property name
				var portView = portViews?.FirstOrDefault(p => p.portData.identifier == property.name);
				if (portView != null && portView.connected)
					continue;

				float h = editor.GetPropertyHeight(property, property.displayName);
				Rect r = EditorGUILayout.GetControlRect(true, h);

				if (property.name.Contains("Vector2"))
					property.vectorValue = (Vector4)EditorGUI.Vector2Field(r, property.displayName, (Vector2)property.vectorValue);
				else if (property.name.Contains("Vector3"))
					property.vectorValue = (Vector4)EditorGUI.Vector3Field(r, property.displayName, (Vector3)property.vectorValue);
				else
					editor.ShaderProperty(r, property, property.displayName);
			}

			return propertiesChanged;
		}

		// Custom property draw, we don't want things that are connected to an edge or useless like the render queue
		protected int GetMaterialHash(Material material)
		{
			int hash = 0;

			if (material == null || material.shader == null)
				return hash;
				
			MaterialProperty[] properties = MaterialEditor.GetMaterialProperties(new []{material});
			var portViews = GetPortViewsFromFieldName(nameof(ShaderNode.materialInputs));

			foreach (var property in properties)
			{
				if ((property.flags & (MaterialProperty.PropFlags.HideInInspector | MaterialProperty.PropFlags.PerRendererData)) != 0)
					continue;

				// Retrieve the port view from the property name
				var portView = portViews?.FirstOrDefault(p => p.portData.identifier == property.name);
				if (portView != null && portView.connected)
					continue;

				switch (property.type)
				{
					case MaterialProperty.PropType.Float:
						hash += property.floatValue.GetHashCode();
						break;
					case MaterialProperty.PropType.Color:
						hash += property.colorValue.GetHashCode();
						break;
					case MaterialProperty.PropType.Range:
						hash += property.rangeLimits.GetHashCode();
						hash += property.floatValue.GetHashCode();
						break;
					case MaterialProperty.PropType.Vector:
						hash += property.vectorValue.GetHashCode();
						break;
					case MaterialProperty.PropType.Texture:
						hash += property.textureValue?.GetHashCode() ?? 0;
						hash += property.textureScaleAndOffset.GetHashCode();
						hash += property.textureDimension.GetHashCode();
						break;
				}
			}

			return hash;
		}

		internal void PinView()
		{
			nodeTarget.isPinned = true;
			pinIcon.tintColor = new Color32(245, 127, 23, 255);
			pinIcon.image = MixtureEditorUtils.unpinIcon;
			schedule.Execute(() => {
				owner.mixtureNodeInspector.AddPinnedView(this);
			}).ExecuteLater(1);
		}

		internal void UnpinView()
		{
			owner.mixtureNodeInspector.RemovePinnedView(this);
			nodeTarget.isPinned = false;
			pinIcon.tintColor = Color.white;
			pinIcon.image = MixtureEditorUtils.pinIcon;
			pinIcon.transform.rotation = Quaternion.identity;
		}

		protected void CreateTexturePreview(VisualElement previewContainer, MixtureNode node, int currentSlice = 0)
		{
			previewContainer.Clear();

			if (node.previewTexture == null)
				return;

			VisualElement texturePreview = new VisualElement();
			previewContainer.Add(texturePreview);

			CreateTexturePreviewImGUI(texturePreview, node, currentSlice);

            previewContainer.name = node.previewTexture.dimension.ToString();

			Button togglePreviewButton = null;
			togglePreviewButton = new Button(() => {
				nodeTarget.isPreviewCollapsed = !nodeTarget.isPreviewCollapsed;
				UpdatePreviewCollapseState();
			});
			togglePreviewButton.ClearClassList();
			togglePreviewButton.AddToClassList("PreviewToggleButton");
			previewContainer.Add(togglePreviewButton);

			UpdatePreviewCollapseState();

			void UpdatePreviewCollapseState()
			{
				if (!nodeTarget.isPreviewCollapsed)
				{
					texturePreview.style.display = DisplayStyle.Flex;
					togglePreviewButton.RemoveFromClassList("Collapsed");
                    nodeTarget.previewVisible = true;
				}
				else
				{
					texturePreview.style.display = DisplayStyle.None;
					togglePreviewButton.AddToClassList("Collapsed");
                    nodeTarget.previewVisible = false;
				}
			}
        }

		Rect GetPreviewRect(Texture texture)
		{
			float width = nodeTarget.nodeWidth; // force preview in width
			float scaleFactor = width / texture.width;
			float height = Mathf.Min(nodeTarget.nodeWidth, texture.height * scaleFactor);
			return GUILayoutUtility.GetRect(1, width, 1, height);
		}

		void DrawPreviewCommonSettings(Texture texture)
		{
			GUILayout.Space(6);

			if (Event.current.type == EventType.KeyDown)
			{
				if (Event.current.keyCode == KeyCode.Delete)
					owner.DelayedDeleteSelection();
			}

			using(new GUILayout.HorizontalScope(EditorStyles.toolbar, GUILayout.Height(12)))
			{
				EditorGUI.BeginChangeCheck();

				bool r = GUILayout.Toggle( (nodeTarget.previewMode & PreviewChannels.R) != 0,"R", EditorStyles.toolbarButton);
				bool g = GUILayout.Toggle( (nodeTarget.previewMode & PreviewChannels.G) != 0,"G", EditorStyles.toolbarButton);
				bool b = GUILayout.Toggle( (nodeTarget.previewMode & PreviewChannels.B) != 0,"B", EditorStyles.toolbarButton);
				bool a = GUILayout.Toggle( (nodeTarget.previewMode & PreviewChannels.A) != 0,"A", EditorStyles.toolbarButton);

				if (EditorGUI.EndChangeCheck())
				{
					owner.RegisterCompleteObjectUndo("Updated Preview Masks");
					nodeTarget.previewMode =
					(r ? PreviewChannels.R : 0) |
					(g ? PreviewChannels.G : 0) |
					(b ? PreviewChannels.B : 0) |
					(a ? PreviewChannels.A : 0);
				}

				if (texture.mipmapCount > 1)
				{
					GUILayout.Space(8);

					nodeTarget.previewMip = GUILayout.HorizontalSlider(nodeTarget.previewMip, 0.0f, texture.mipmapCount - 1, GUILayout.Width(64));
					GUILayout.Label($"Mip #{Mathf.RoundToInt(nodeTarget.previewMip)}", EditorStyles.toolbarButton);
				}

				GUILayout.FlexibleSpace();
			}
		}

		void DrawTextureInfoHover(Rect previewRect, Texture texture)
		{
			Rect infoRect = previewRect;
			infoRect.yMin += previewRect.height - 24;
			infoRect.height = 20;
			previewRect.yMax -= 4;

			// On Hover : Transparent Bar for Preview with information
			if (previewRect.Contains(Event.current.mousePosition) && !infoRect.Contains(Event.current.mousePosition))
			{
				EditorGUI.DrawRect(infoRect, new Color(0, 0, 0, 0.65f));

				infoRect.xMin += 8;

				// Shadow
				GUI.color = Color.white;
				GUI.Label(infoRect, $"{texture.width}x{texture.height} - {nodeTarget.rtSettings.GetGraphicsFormat(owner.graph)}", EditorStyles.boldLabel);
			}
		}

		void CreateTexturePreviewImGUI(VisualElement previewContainer, MixtureNode node, int currentSlice)
		{
			// Add slider for texture 3D
			if (node.previewTexture.dimension == TextureDimension.Tex3D)
			{
				var previewSliceIndex = new SliderInt(0, TextureUtils.GetSliceCount(node.previewTexture) - 1)
				{
					label = "Slice",
					value = currentSlice,
				};
				previewSliceIndex.RegisterValueChangedCallback((ChangeEvent< int > a) => {
					currentSlice = a.newValue;
				});
				previewContainer.Add(previewSliceIndex);
			}
			if (node.showPreviewExposure)
			{
				var previewExposure = new Slider(0, 10)
				{
					label = "Preview EV100",
					value = node.previewEV100,
				};
				previewExposure.RegisterValueChangedCallback(e => {
					node.previewEV100 = e.newValue;
				});
				previewContainer.Add(previewExposure);
			}

			var previewImageSlice = new IMGUIContainer(() => {
				if (node.previewTexture == null)
					return;

				DrawPreviewCommonSettings(node.previewTexture);

				Rect previewRect = GetPreviewRect(node.previewTexture);
				DrawImGUIPreview(node, previewRect, currentSlice);

				DrawTextureInfoHover(previewRect, node.previewTexture);
            });

			// Force the ImGUI preview to refresh
			EditorApplication.update -= previewImageSlice.MarkDirtyRepaint;
			EditorApplication.update += previewImageSlice.MarkDirtyRepaint;

			previewContainer.Add(previewImageSlice);
		}

		protected virtual void DrawImGUIPreview(MixtureNode node, Rect previewRect, int currentSlice)
		{
			switch (node.previewTexture.dimension)
			{
				case TextureDimension.Tex2D:
					MixtureUtils.texture2DPreviewMaterial.SetTexture("_MainTex", node.previewTexture);
					MixtureUtils.texture2DPreviewMaterial.SetVector("_Size", new Vector4(node.previewTexture.width,node.previewTexture.height, 1, 1));
					MixtureUtils.texture2DPreviewMaterial.SetVector("_Channels", MixtureEditorUtils.GetChannelsMask(nodeTarget.previewMode));
					MixtureUtils.texture2DPreviewMaterial.SetFloat("_PreviewMip", nodeTarget.previewMip);
					MixtureUtils.texture2DPreviewMaterial.SetFloat("_EV100", nodeTarget.previewEV100);
					MixtureUtils.SetupIsSRGB(MixtureUtils.texture2DPreviewMaterial, nodeTarget, owner.graph);

					if (Event.current.type == EventType.Repaint)
						EditorGUI.DrawPreviewTexture(previewRect, node.previewTexture, MixtureUtils.texture2DPreviewMaterial, ScaleMode.ScaleToFit, 0, 0);
					break;
				case TextureDimension.Tex3D:
					MixtureUtils.texture3DPreviewMaterial.SetTexture("_Texture3D", node.previewTexture);
					MixtureUtils.texture3DPreviewMaterial.SetVector("_Channels", MixtureEditorUtils.GetChannelsMask(nodeTarget.previewMode));
					MixtureUtils.texture3DPreviewMaterial.SetFloat("_PreviewMip", nodeTarget.previewMip);
					MixtureUtils.texture3DPreviewMaterial.SetFloat("_Depth", ((float)currentSlice + 0.5f) / nodeTarget.rtSettings.GetDepth(owner.graph));
					MixtureUtils.texture3DPreviewMaterial.SetFloat("_EV100", nodeTarget.previewEV100);
					MixtureUtils.SetupIsSRGB(MixtureUtils.texture3DPreviewMaterial, nodeTarget, owner.graph);

					if (Event.current.type == EventType.Repaint)
						EditorGUI.DrawPreviewTexture(previewRect, Texture2D.whiteTexture, MixtureUtils.texture3DPreviewMaterial, ScaleMode.ScaleToFit, 0, 0, ColorWriteMask.Red);
					break;
				case TextureDimension.Cube:
					MixtureUtils.textureCubePreviewMaterial.SetTexture("_Cubemap", node.previewTexture);
					MixtureUtils.textureCubePreviewMaterial.SetVector("_Channels", MixtureEditorUtils.GetChannelsMask(nodeTarget.previewMode));
					MixtureUtils.textureCubePreviewMaterial.SetFloat("_PreviewMip", nodeTarget.previewMip);
					MixtureUtils.textureCubePreviewMaterial.SetFloat("_EV100", nodeTarget.previewEV100);
					MixtureUtils.SetupIsSRGB(MixtureUtils.textureCubePreviewMaterial, nodeTarget, owner.graph);

					if (Event.current.type == EventType.Repaint)
						EditorGUI.DrawPreviewTexture(previewRect, Texture2D.whiteTexture, MixtureUtils.textureCubePreviewMaterial, ScaleMode.ScaleToFit, 0, 0);
					break;
				default:
					Debug.LogError(node.previewTexture + " is not a supported type for preview");
					break;
			}
		}

		void InitProcessingTimeLabel()
		{
			if (processTimeLabel != null)
				return;

			processTimeLabel = new Label();
			processTimeLabel.style.unityTextAlign = TextAnchor.MiddleCenter;

			Add(processTimeLabel);


			schedule.Execute(() => {
				// Update processing time every 200 millis

				float time = nodeTarget.processingTimeInMillis;
				if (time > 0.1f)
				{
					processTimeLabel.text = time.ToString("F2") + " ms";
					// Color range based on the time:
					float weight = time / 30; // We consider 30 ms as slow
					processTimeLabel.style.color = new Color(2.0f * weight, 2.0f * (1 - weight), 0);
				}
			}).Every(200);
		}
	}
}
