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
		static Material	_blitIconMaterial;
		static Material blitIconMaterial
		{
			get
			{
				if (_blitIconMaterial == null)
				{
					// blitIconMaterial = new Material(Shader.Find())
				}

				return _blitIconMaterial;
			}
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

        public override void OnPreviewGUI(Rect r, GUIStyle background)
		{
			if (target != null)
				EditorGUI.DrawPreviewTexture(r, target as Texture2D);
		}

        public override Texture2D RenderStaticPreview(string assetPath, UnityEngine.Object[] subAssets, int width, int height)
		{
			Texture2D		icon = new Texture2D(width, height);
			RenderTexture	rt = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);

			// TODO
			// Graphics.Blit(target as Texture2D, rt, blitIconMaterial);
			Graphics.Blit(target as Texture2D, rt);

			RenderTexture.active = rt;
			icon.ReadPixels(new Rect(0, 0, width, height), 0, 0);
			icon.Apply();

			return icon;
		}
	}

	[CustomEditor(typeof(Texture2DArray), false)]
	public class MixtureInspectorTexture2DArray : Editor
	{
		Material		_textureArrayPreviewMaterial;
		Material		textureArrayPreviewMaterial
		{
			get
			{
				if (_textureArrayPreviewMaterial == null)
				{
					_textureArrayPreviewMaterial = new Material(Shader.Find("Hidden/MixtureTextureArrayPreview"));
				}

				return _textureArrayPreviewMaterial;
			}
		}

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
			slice = EditorGUILayout.IntSlider("Slice", slice, 0, array.depth);
		}

        public override void OnPreviewGUI(Rect r, GUIStyle background)
		{
			textureArrayPreviewMaterial.SetFloat("_Slice", slice);
			EditorGUI.DrawPreviewTexture(r, Texture2D.whiteTexture, textureArrayPreviewMaterial);
		}

		public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height)
		{
			return Texture2D.whiteTexture;
		}
	}
}