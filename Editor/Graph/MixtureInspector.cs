using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;

namespace Mixture
{
	[InitializeOnLoad]
	class MixtureSmallIconRenderer
	{
		static HashSet< string >	mixtureAssets = new HashSet< string >();

		static MixtureSmallIconRenderer() => EditorApplication.projectWindowItemOnGUI += DrawMixtureSmallIcon;
		
		static void DrawMixtureSmallIcon(string assetGUID, Rect rect)
		{
			// If the icon is not small
			if (rect.height != 16)
				return ;
			
			if (mixtureAssets.Contains(assetGUID))
				DrawMixtureSmallIcon(rect);
			
			string assetPath = AssetDatabase.GUIDToAssetPath(assetGUID);

			// Mixture assets are saved as .asset files
			if (!assetPath.EndsWith($".{MixtureAssetCallbacks.Extension}"))
				return ;

			// ensure that the asset is a texture:
			if (AssetDatabase.LoadAssetAtPath< Texture >(assetPath) == null)
				return ;

			mixtureAssets.Add(assetGUID);
			DrawMixtureSmallIcon(rect);
		}

		static void DrawMixtureSmallIcon(Rect rect)
		{
			Rect clearRect = new Rect(rect.x, rect.y, 20, 16);
			Rect iconRect = new Rect(rect.x + 2, rect.y, 16, 16);

			// Draw a quad of the color of the background
			Color backgroundColor = EditorGUIUtility.isProSkin
				? new Color32(56, 56, 56, 255)
				: new Color32(194, 194, 194, 255);

			EditorGUI.DrawRect(clearRect, backgroundColor);
			GUI.DrawTexture(iconRect, MixtureUtils.icon32);
		}
	}

	public class MixtureEditor : Editor
	{
		public virtual void OnEnable() {}

		protected void BlitMixtureIcon(Texture preview, RenderTexture target)
		{
			MixtureUtils.SetupDimensionKeyword(MixtureUtils.blitIconMaterial, preview.dimension);

			switch (preview.dimension)
			{
				case TextureDimension.Tex2D:
					MixtureUtils.blitIconMaterial.SetTexture("_Texture2D", preview);
					Graphics.Blit(preview, target, MixtureUtils.blitIconMaterial, 0);
					break;
				case TextureDimension.Tex2DArray:
					MixtureUtils.blitIconMaterial.SetTexture("_Texture2DArray", preview);
					Graphics.Blit(preview, target, MixtureUtils.blitIconMaterial, 0);
					break;
				case TextureDimension.Tex3D:
					MixtureUtils.blitIconMaterial.SetTexture("_Texture3D", preview);
					Graphics.Blit(preview, target, MixtureUtils.blitIconMaterial, 0);
					break;
				case TextureDimension.Cube:
					MixtureUtils.blitIconMaterial.SetTexture("_Cubemap", preview);
					Graphics.Blit(preview, target, MixtureUtils.blitIconMaterial, 0);
					break;
				default:
					Debug.LogError($"{preview.dimension} is not supported for icon preview");
					break;
			}
		}

		public override void OnInspectorGUI()
		{
			Texture t = target as Texture;

			t.wrapMode = (TextureWrapMode)EditorGUILayout.EnumPopup("Wrap Mode", t.wrapMode);
			t.filterMode = (FilterMode)EditorGUILayout.EnumPopup("Filter Mode", t.filterMode);
			t.anisoLevel = EditorGUILayout.IntSlider("Aniso Level", t.anisoLevel, 1, 9);
		}
	}

	// By default textures don't have any CustomEditors so we can define them for Mixture
	[CustomEditor(typeof(Texture2D), false)]
	public class MixtureInspectorTexture2D : MixtureEditor
	{
		public override void DrawPreview(Rect previewArea)
		{
			OnPreviewGUI(previewArea, GUIStyle.none);
		}

		public override bool HasPreviewGUI() => true;

		// TODO: use the preview of the true Texture2D inspector
        public override void OnPreviewGUI(Rect r, GUIStyle background)
		{
			if (target != null)
				EditorGUI.DrawPreviewTexture(r, target as Texture2D);
		}

        public override Texture2D RenderStaticPreview(string assetPath, UnityEngine.Object[] subAssets, int width, int height)
		{
			Texture2D		icon = new Texture2D(width, height);
			RenderTexture	rt = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
			
			if (target == null)
				target = AssetDatabase.LoadAssetAtPath< Texture2D >(assetPath);

			// Texture2D could be a standard unity texture, in this case we don't want the mixture icon on it
			if (assetPath.EndsWith(".asset")) // If the texture is an asset, then it means that it have not been imported from png, jpg, ...
				BlitMixtureIcon(target as Texture, rt);
			else
				Graphics.Blit(target as Texture, rt);

			RenderTexture.active = rt;
			icon.ReadPixels(new Rect(0, 0, width, height), 0, 0);
			icon.Apply();
			RenderTexture.active = null;
			rt.Release();

			return icon;
		}
	}

	[CustomEditor(typeof(Texture2DArray), false)]
	public class MixtureInspectorTexture2DArray : MixtureEditor
	{
		Texture2DArray	array;
		int				slice;

		public override void OnEnable()
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
	public class MixtureInspectorTexture3D : MixtureEditor
	{
		Texture3D	volume;
		int			slice;

		public override void OnEnable()
		{
			base.OnEnable();
			volume = target as Texture3D;
		}

		public override void DrawPreview(Rect previewArea)
		{
			OnPreviewGUI(previewArea, GUIStyle.none);
		}

		public override bool HasPreviewGUI() => true;

		public override void OnPreviewSettings()
		{
			EditorGUIUtility.labelWidth = 30;
			slice = EditorGUILayout.IntSlider("Slice", slice, 0, volume.depth - 1);
		}

        public override void OnPreviewGUI(Rect r, GUIStyle background)
		{
			float depth = ((float)slice + 0.5f) / (float)volume.depth;
			MixtureUtils.texture3DPreviewMaterial.SetFloat("_Depth", depth);
			MixtureUtils.texture3DPreviewMaterial.SetTexture("_Texture3D", volume);
			EditorGUI.DrawPreviewTexture(r, Texture2D.whiteTexture, MixtureUtils.texture3DPreviewMaterial);
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

	[CustomEditor(typeof(Cubemap), false)]
	public class MixtureInspectorTextureCube : MixtureEditor
	{
		Cubemap		cubemap;
		int			slice;

		public override void OnEnable()
		{
			base.OnEnable();
			cubemap = target as Cubemap;
		}

		public override void DrawPreview(Rect previewArea)
		{
			OnPreviewGUI(previewArea, GUIStyle.none);
		}

		public override bool HasPreviewGUI() => true;

		public override void OnPreviewSettings() {}

        public override void OnPreviewGUI(Rect r, GUIStyle background)
		{
			MixtureUtils.textureCubePreviewMaterial.SetTexture("_Cubemap", cubemap);
			EditorGUI.DrawPreviewTexture(r, Texture2D.whiteTexture, MixtureUtils.textureCubePreviewMaterial);
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
}