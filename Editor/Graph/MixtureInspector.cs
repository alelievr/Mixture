using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;

namespace Mixture
{
	// By default textures don't have any CustomEditors so we can define them for Mixture
	[CustomEditor(typeof(Texture2D), false)]
	public class MixtureInspectorTexture2D : Editor
	{
		public override void OnInspectorGUI()
		{
			EditorGUILayout.LabelField("Hello World !");
		}

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

			Graphics.Blit(target as Texture2D, rt);

			RenderTexture.active = rt;
			icon.ReadPixels(new Rect(0, 0, width, height), 0, 0);
			icon.Apply();
			RenderTexture.active = null;
			rt.Release();

			return icon;
		}
	}

	[CustomEditor(typeof(Texture2DArray), false)]
	public class MixtureInspectorTexture2DArray : Editor
	{
		Texture2DArray	array;
		int				slice;

		void OnEnable()
		{
			array = target as Texture2DArray;
		}

		public override void OnInspectorGUI()
		{
			EditorGUILayout.LabelField("Hello World !");
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

			Graphics.Blit(array, rt, 0, 0);

			RenderTexture.active = rt;
			icon.ReadPixels(new Rect(0, 0, width, height), 0, 0);
			icon.Apply();
			RenderTexture.active = null;
			rt.Release();

			return icon;
		}
	}
}