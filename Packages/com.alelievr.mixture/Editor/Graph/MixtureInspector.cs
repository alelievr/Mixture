using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using System;
using System.Linq;
using System.Collections.Generic;
using Object = UnityEngine.Object;
using UnityEngine.UIElements;
using GraphProcessor;
using UnityEditor.UIElements;

namespace Mixture
{
	[InitializeOnLoad]
	class MixtureSmallIconRenderer
	{
		static Dictionary< string, MixtureGraph >	mixtureAssets = new Dictionary< string, MixtureGraph >();
		static Dictionary< string, MixtureVariant >	mixtureVariants = new Dictionary< string, MixtureVariant >();

		static MixtureSmallIconRenderer() => EditorApplication.projectWindowItemOnGUI += DrawMixtureSmallIcon;
		
		static void DrawMixtureSmallIcon(string assetGUID, Rect rect)
		{
			// If the icon is not small
			if (rect.height != 16)
				return ;
			
			MixtureGraph graph;
			if (mixtureAssets.TryGetValue(assetGUID, out graph))
			{
				DrawMixtureSmallIcon(rect, graph);
				return ;
			}

			if (mixtureVariants.TryGetValue(assetGUID, out var v))
			{
				// TODO: draw the mixture variant icon
				DrawMixtureSmallIcon(rect, v);
				return ;
			}

			string assetPath = AssetDatabase.GUIDToAssetPath(assetGUID);

			// Mixture assets are saved as .asset files
			if (!assetPath.EndsWith($".{MixtureAssetCallbacks.Extension}"))
				return ;

			// ensure that the asset is a texture:
			var texture = AssetDatabase.LoadAssetAtPath< Texture >(assetPath);
			if (texture == null)
				return ;

			// Check if the current texture is a mixture variant
			var variant = MixtureEditorUtils.GetVariantAtPath(assetPath);
			if (variant != null)
			{
				mixtureVariants.Add(assetGUID, variant);
				DrawMixtureSmallIcon(rect, variant);
				return;
			}

			// and then that it have a Mixture Graph as subasset
			graph = MixtureEditorUtils.GetGraphAtPath(assetPath);
			if (graph != null)
			{
				mixtureAssets.Add(assetGUID, graph);
				DrawMixtureSmallIcon(rect, graph);
				return ;
			}
		}

		static void DrawMixtureSmallIcon(Rect rect, MixtureGraph graph)
			=> DrawMixtureSmallIcon(rect, graph.isRealtime ? MixtureUtils.realtimeIcon32 : MixtureUtils.icon32);

		static void DrawMixtureSmallIcon(Rect rect, MixtureVariant variant)
			=> DrawMixtureSmallIcon(rect, variant.parentGraph.isRealtime ? MixtureUtils.realtimeVariantIcon32 : MixtureUtils.iconVariant32);

		static void DrawMixtureSmallIcon(Rect rect, Texture2D mixtureIcon)
		{
			Rect clearRect = new Rect(rect.x, rect.y, 20, 16);
			Rect iconRect = new Rect(rect.x + 2, rect.y, 16, 16);

			// TODO: find a way to detect the focus of the project window instantaneously
			bool focused = false;

			// Draw a quad of the color of the background
			Color backgroundColor;
			if (EditorGUIUtility.isProSkin)
				backgroundColor = focused ? new Color32(44, 93, 135, 255) : new Color32(56, 56, 56, 255);
			else
				backgroundColor = new Color32(194, 194, 194, 255);

			EditorGUI.DrawRect(clearRect, backgroundColor);
			GUI.DrawTexture(iconRect, mixtureIcon);
		}
	}

	class MixtureEditor : Editor
	{
		protected Editor		defaultTextureEditor;
		protected Editor		variantEditor;
		protected MixtureGraph	graph;
		protected MixtureVariant variant;
		protected VisualElement	root;
		protected VisualElement	parameters;
        protected ExposedParameterFieldFactory exposedParameterFactory;

		protected virtual void OnEnable()
		{
			// Load the mixture graph:
			graph = MixtureEditorUtils.GetGraphAtPath(AssetDatabase.GetAssetPath(target));
			variant = MixtureEditorUtils.GetVariantAtPath(AssetDatabase.GetAssetPath(target));

			if (graph != null)
			{
				exposedParameterFactory = new ExposedParameterFieldFactory(graph);
				graph.onExposedParameterListChanged += UpdateExposedParameters;
				graph.onExposedParameterModified += UpdateExposedParameters;
			}
			if (variant != null)
			{
				graph = variant.parentGraph;
				exposedParameterFactory = new ExposedParameterFieldFactory(variant.parentGraph);
				Editor.CreateCachedEditor(variant, typeof(MixtureVariantInspector), ref variantEditor);
			}
		}

		static Dictionary< Type, string > defaultTextureInspectors = new Dictionary< Type, string >()
		{
			{ typeof(Texture2D), "UnityEditor.TextureInspector"},
			{ typeof(Texture3D), "UnityEditor.Texture3DInspector"},
			{ typeof(Cubemap), "UnityEditor.CubemapInspector"},
			{ typeof(CustomRenderTexture), "UnityEditor.CustomRenderTextureEditor"},
			{ typeof(Material), "UnityEditor.MaterialEditor" },
		};

		protected virtual void LoadInspectorFor(Type typeForEditor, Object[] targets)
		{
			string editorTypeName;
			if (defaultTextureInspectors.TryGetValue(typeForEditor, out editorTypeName))
			{
				foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
				{
					var editorType = assembly.GetType(editorTypeName);
					if (editorType != null)
					{
						Editor.CreateCachedEditor(targets, editorType, ref defaultTextureEditor);
						return ;
					}
				}
			}
		}

		protected virtual void OnDisable()
		{
			if (graph != null)
			{
				graph.onExposedParameterListChanged -= UpdateExposedParameters;
				graph.onExposedParameterModified -= UpdateExposedParameters;
				exposedParameterFactory.Dispose();
				exposedParameterFactory = null;
			}

			if (defaultTextureEditor != null)
				DestroyImmediate(defaultTextureEditor);
			if (variantEditor != null)
				DestroyImmediate(variantEditor);
		}
		
		// This block of functions allow us to use the default behavior of the texture inspector instead of re-writing
		// the preview / static icon code for each texture type, we use the one from the default texture inspector.
		public override void DrawPreview(Rect previewArea) { if (defaultTextureEditor != null) defaultTextureEditor.DrawPreview(previewArea); else base.DrawPreview(previewArea); }
		public override string GetInfoString() { if (defaultTextureEditor != null) return defaultTextureEditor.GetInfoString(); else return base.GetInfoString(); }
		public override GUIContent GetPreviewTitle() { if (defaultTextureEditor != null) return defaultTextureEditor.GetPreviewTitle(); else return base.GetPreviewTitle(); }
		public override bool HasPreviewGUI() { if (defaultTextureEditor != null) return defaultTextureEditor.HasPreviewGUI(); else return base.HasPreviewGUI(); }
		public override void OnInteractivePreviewGUI(Rect r, GUIStyle background) { if (defaultTextureEditor != null) defaultTextureEditor.OnInteractivePreviewGUI(r, background); else base.OnInteractivePreviewGUI(r, background); }
		public override void OnPreviewGUI(Rect r, GUIStyle background) { if (defaultTextureEditor != null) defaultTextureEditor.OnPreviewGUI(r, background); else base.OnPreviewGUI(r, background); }
		public override void OnPreviewSettings() { if (defaultTextureEditor != null) defaultTextureEditor.OnPreviewSettings(); else base.OnPreviewSettings(); }
		public override void ReloadPreviewInstances() { if (defaultTextureEditor != null) defaultTextureEditor.ReloadPreviewInstances(); else base.ReloadPreviewInstances(); }
		public override bool RequiresConstantRepaint() { if (defaultTextureEditor != null) return defaultTextureEditor.RequiresConstantRepaint(); else return base.RequiresConstantRepaint(); }
		public override bool UseDefaultMargins() { if (defaultTextureEditor != null) return defaultTextureEditor.UseDefaultMargins(); else return base.UseDefaultMargins(); }

		protected void BlitMixtureIcon(Texture preview, RenderTexture target, bool realtime = false)
		{
			var blitMaterial = (realtime) ? MixtureUtils.blitRealtimeIconMaterial : MixtureUtils.blitIconMaterial;
			MixtureUtils.SetupDimensionKeyword(blitMaterial, preview.dimension);

			switch (preview.dimension)
			{
				case TextureDimension.Tex2D:
					blitMaterial.SetTexture("_Texture2D", preview);
					Graphics.Blit(preview, target, blitMaterial, 0);
					break;
				case TextureDimension.Tex2DArray:
					blitMaterial.SetTexture("_Texture2DArray", preview);
					Graphics.Blit(preview, target, blitMaterial, 0);
					break;
				case TextureDimension.Tex3D:
					blitMaterial.SetTexture("_Texture3D", preview);
					Graphics.Blit(preview, target, blitMaterial, 0);
					break;
				case TextureDimension.Cube:
					blitMaterial.SetTexture("_Cubemap", preview);
					Graphics.Blit(preview, target, blitMaterial, 0);
					break;
				default:
					Debug.LogError($"{preview.dimension} is not supported for icon preview");
					break;
			}
		}

		public override VisualElement CreateInspectorGUI()
		{
			if (graph == null)
				return base.CreateInspectorGUI();
			
			CreateRootElement();

			if (variant != null)
			{
				root.Add(variantEditor.CreateInspectorGUI());
				return root;
			}

			UpdateExposedParameters(null);
			root.Add(CreateTextureSettingsView());
			root.Add(CreateAdvancedSettingsView());

			return root;
		}

		protected void CreateRootElement()
		{
			root = new VisualElement();

			var styleSheet = Resources.Load<StyleSheet>("MixtureInspector");
			if (styleSheet != null)
				root.styleSheets.Add(styleSheet);
		}

		protected void UpdateExposedParameters(ExposedParameter param) => UpdateExposedParameters();
		protected void UpdateExposedParameters()
		{
			if (root == null)
				return;

			if (parameters == null || !root.Contains(parameters))
			{
				parameters = new VisualElement();
				root.Add(parameters);
			}

			parameters.Clear();

			bool header = true;
			bool showUpdateButton = false;
			foreach (var param in graph.exposedParameters)
            {
                if (param.settings.isHidden)
                    continue;
				
				if (header)
				{
					var headerLabel = new Label("Exposed Parameters");
					headerLabel.AddToClassList("Header");
					parameters.Add(headerLabel);
					header = false;
					showUpdateButton = true;
				}
                VisualElement prop = new VisualElement();
				prop.AddToClassList("Indent");
                prop.style.display = DisplayStyle.Flex;
                var p = exposedParameterFactory.GetParameterValueField(param, (newValue) => {
                    param.value = newValue;
                    graph.NotifyExposedParameterValueChanged(param);
                });
                prop.Add(p);
                parameters.Add(prop);
            }

			if (showUpdateButton)
			{
				var updateButton = new Button(() => {
					MixtureGraphProcessor.RunOnce(graph);
					graph.SaveAllTextures(false);
				}) { text = "Update Texture(s)" };
				updateButton.AddToClassList("Indent");
				parameters.Add(updateButton);
			}
		}

		VisualElement CreateTextureSettingsView()
		{
			var textureSettings = new VisualElement();

			var t = target as Texture;

			var settingsLabel = new Label("Texture Settings");
			settingsLabel.AddToClassList("Header");
			textureSettings.Add(settingsLabel);

			var settings = new VisualElement();
			settings.AddToClassList("Indent");
			textureSettings.Add(settings);

			var wrapMode = new EnumField("Wrap Mode", t.wrapMode);
			wrapMode.RegisterValueChangedCallback(e => {
				Undo.RegisterCompleteObjectUndo(t, "Changed wrap mode");
				t.wrapMode = (TextureWrapMode)e.newValue;
			});
			settings.Add(wrapMode);

			var filterMode = new EnumField("Filter Mode", t.filterMode);
			filterMode.RegisterValueChangedCallback(e => {
				Undo.RegisterCompleteObjectUndo(t, "Changed filter mode");
				t.filterMode = (FilterMode)e.newValue;
			});
			settings.Add(filterMode);

			var aniso = new SliderInt("Aniso Level", 1, 9);
			aniso.RegisterValueChangedCallback(e => {
				Undo.RegisterCompleteObjectUndo(t, "Changed aniso level");
				t.anisoLevel = e.newValue;
			});
			settings.Add(aniso);

			return textureSettings;
		}

		VisualElement CreateAdvancedSettingsView()
		{
			var advanced = new VisualElement();
			var container = new VisualElement();
			container.AddToClassList("Indent");

			if (!graph.isRealtime)
			{
				var embed = new Toggle("Embed Graph In Build") { value = graph.embedInBuild };
				embed.RegisterValueChangedCallback(e => {
					Undo.RegisterCompleteObjectUndo(graph, "Changed Embed In Build");
					graph.embedInBuild = e.newValue;
				});
				container.Add(embed);
			}

			container.Add(new Button(CreateMixtureVariant) { text = "Create New Mixture Variant"});

			var advancedLabel = new Label("Advanced Settings");
			advancedLabel.AddToClassList("Header");
			advanced.Add(advancedLabel);

			advanced.Add(container);

			return advanced;
		}

		void CreateMixtureVariant() => MixtureAssetCallbacks.CreateMixtureVariant(graph, null);
		
		public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height)
		{
			// If the CRT is not a realtime mixture, then we display the default inspector
			if (defaultTextureEditor == null)
			{
				Debug.LogError("Can't generate static preview for asset " + target);
				return base.RenderStaticPreview(assetPath, subAssets, width, height);
			}

			var defaultPreview = defaultTextureEditor.RenderStaticPreview(assetPath, subAssets, width, height);

			if (graph == null)
				return defaultPreview;
			
			// Combine manually on CPU the two textures because it completely broken with GPU :'(
			Texture2D mixtureIcon = (target is CustomRenderTexture) ? MixtureUtils.realtimeIcon : MixtureUtils.icon;
			if (target is MixtureVariant v)
				mixtureIcon = graph.isRealtime ? MixtureUtils.realtimeVariantIcon : MixtureUtils.iconVariant;

			float scaleFactor = Mathf.Max(mixtureIcon.width / (float)defaultPreview.width, 1) * 2.5f;
			for (int x = 0; x < width / 2.5f; x++)
			for (int y = 0; y < height / 2.5f; y++)
			{
				var iconColor = mixtureIcon.GetPixel((int)(x * scaleFactor), (int)(y * scaleFactor));
				var color = Color.Lerp(defaultPreview.GetPixel(x, y), iconColor, iconColor.a);
				defaultPreview.SetPixel(x, y, color);
			}

			defaultPreview.Apply();

			return defaultPreview;
		}
	}

	// By default textures don't have any CustomEditors so we can define them for Mixture
	[CustomEditor(typeof(Texture2D), false)]
	class MixtureInspectorTexture2D : MixtureEditor
	{
		protected override void OnEnable()
		{
			base.OnEnable();
			LoadInspectorFor(typeof(Texture2D), targets);
		}
	}

	[CustomEditor(typeof(Texture2DArray), false)]
	class MixtureInspectorTexture2DArray : MixtureEditor
	{
		Texture2DArray	array;
		int				slice;

		protected override void OnEnable()
		{
			base.OnEnable();
			array = target as Texture2DArray;
		}

		public override void DrawPreview(Rect previewArea)
		{
			OnPreviewGUI(previewArea, GUIStyle.none);
		}

		public override bool HasPreviewGUI() => true;

		public override void OnPreviewSettings()
		{
			EditorGUIUtility.labelWidth = 30;
			slice = EditorGUILayout.IntSlider("Slice", slice, 0, array.depth - 1);
		}

        public override void OnPreviewGUI(Rect r, GUIStyle background)
		{
			MixtureUtils.textureArrayPreviewMaterial.SetFloat("_Slice", slice);
			MixtureUtils.textureArrayPreviewMaterial.SetTexture("_TextureArray", array);
			EditorGUI.DrawPreviewTexture(r, Texture2D.whiteTexture, MixtureUtils.textureArrayPreviewMaterial);
		}

		public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height)
		{
			var icon = new Texture2D(width, height);
			RenderTexture	rt = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);

			BlitMixtureIcon(target as Texture, rt);

			RenderTexture.active = rt;
			icon.ReadPixels(new Rect(0, 0, width, height), 0, 0);
			icon.Apply();
			RenderTexture.active = null;
			rt.Release();

			return icon;
		}
	}

	[CustomEditor(typeof(Texture3D), false)]
	class MixtureInspectorTexture3D : MixtureEditor
	{
		Texture3D	volume;
		int			slice = 0;

		protected override void OnEnable()
		{
			base.OnEnable();
			volume = target as Texture3D;
			LoadInspectorFor(typeof(Texture3D), targets);
		}

        public override void OnInspectorGUI()
        {
			defaultTextureEditor.OnInspectorGUI();
        }

		public override bool HasPreviewGUI() => graph != null ? true : base.HasPreviewGUI();

        public override void OnPreviewGUI(Rect r, GUIStyle background)
		{
			if (graph == null)
			{
				base.OnPreviewGUI(r, background);
				return;
			}

			float depth = ((float)slice + 0.5f) / (float)volume.depth;
			MixtureUtils.texture3DPreviewMaterial.SetFloat("_Depth", depth);
			MixtureUtils.texture3DPreviewMaterial.SetTexture("_Texture3D", volume);
			EditorGUI.DrawPreviewTexture(r, Texture2D.whiteTexture, MixtureUtils.texture3DPreviewMaterial);
		}
	}

	[CustomEditor(typeof(Cubemap), false)]
	class MixtureInspectorTextureCube : MixtureEditor
	{
		Cubemap		cubemap;
		int			slice;

		protected override void OnEnable()
		{
			base.OnEnable();
			cubemap = target as Cubemap;
			LoadInspectorFor(typeof(Cubemap), targets);
		}
	}
	
	[CustomEditor(typeof(CustomRenderTexture), false)]
	class RealtimeMixtureInspector : MixtureEditor
	{
		CustomRenderTexture	crt;
		bool				isMixture;

		protected override void OnEnable()
		{
			base.OnEnable();
			base.LoadInspectorFor(typeof(CustomRenderTexture), targets);
			crt = target as CustomRenderTexture;

			ReloadPreviewInstances();

			isMixture = RealtimeMixtureReferences.realtimeMixtureCRTs.Contains(crt);
		}

        public override bool RequiresConstantRepaint() => true;

		public override void OnInspectorGUI()
        {
			defaultTextureEditor.OnInspectorGUI();
        }

		public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height)
		{
			// If the CRT is not a realtime mixture, then we display the default inspector
			if (!isMixture)
				return defaultTextureEditor.RenderStaticPreview(assetPath, subAssets, width, height);
			return base.RenderStaticPreview(assetPath, subAssets, width, height);
		}
	}
}