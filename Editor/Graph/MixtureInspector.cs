using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using System.Linq;

namespace Mixture
{
	public class MixtureEditor : Editor
	{
		public virtual void OnEnable()
		{
			 EditorApplication.projectWindowItemOnGUI += DrawSmallMixtureIcon;
		}

		void DrawSmallMixtureIcon(string guid, Rect rect)
		{
            bool isSmall = rect.width > rect.height;
			int iconSize = (int)EditorGUIUtility.singleLineHeight;

			// We only display the mixture icon when we're in small mode
			if (isSmall)
			{
				var path = AssetDatabase.GUIDToAssetPath(guid);
				var texture = AssetDatabase.LoadAssetAtPath(path, typeof(Texture));
				var mixtureRect = new Rect(rect.x + 3, rect.y, rect.width, rect.height);

				if (texture == null)
					return;

				var allAssets = AssetDatabase.LoadAllAssetsAtPath(path);
				if (!allAssets.Any(a => a is MixtureGraph))
					return;

				mixtureRect.width = iconSize;
				mixtureRect.height = iconSize;

				GUI.DrawTexture(mixtureRect, MixtureUtils.icon);
			}
		}

		protected void BlitMixtureIcon(Texture preview, RenderTexture target)
		{
			// Disable all material keywords:
			foreach (var keyword in MixtureUtils.blitIconMaterial.shaderKeywords)
				MixtureUtils.blitIconMaterial.DisableKeyword(keyword);

			switch (preview.dimension)
			{
				case TextureDimension.Tex2D:
					MixtureUtils.blitIconMaterial.SetTexture("_Texture2D", preview);
					MixtureUtils.blitIconMaterial.EnableKeyword("TEXTURE2D");
					Graphics.Blit(preview, target, MixtureUtils.blitIconMaterial, 0);
					break;
				case TextureDimension.Tex2DArray:
					MixtureUtils.blitIconMaterial.SetTexture("_Texture2DArray", preview);
					MixtureUtils.blitIconMaterial.EnableKeyword("TEXTURE2D_ARRAY");
					Graphics.Blit(preview, target, MixtureUtils.blitIconMaterial, 0);
					break;
				case TextureDimension.Tex3D:
					MixtureUtils.blitIconMaterial.SetTexture("_Texture3D", preview);
					MixtureUtils.blitIconMaterial.EnableKeyword("TEXTURE3D");
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

			EditorGUILayout.LabelField("Hello World !");

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
			if (!subAssets.Any(s => s is Material))
				Graphics.Blit(target as Texture, rt);
			else
				BlitMixtureIcon(target as Texture, rt);

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
			MixtureUtils.texture3DPreviewMaterial.SetFloat("_Slice", slice);
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
}