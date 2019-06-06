using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;

namespace Mixture
{
	// By default textures don't have any CustomEditors so we can define them for Mixture
	// [CustomEditor(typeof(Texture2D), false)]
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

		public override bool HasPreviewGUI() => true;

        // public override void OnPreviewGUI(Rect r, GUIStyle background)
		// {
		// 	// EditorGUI.DrawPreviewTexture(r, MixtureAssetCallbacks.Icon);
		// }

        public override Texture2D RenderStaticPreview(string assetPath, UnityEngine.Object[] subAssets, int width, int height)
		{
			RenderTexture	rt = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
			Graphics.Blit(target as Texture2D, rt, blitIconMaterial);
			return target as Texture2D;
		}
	}

	[CustomEditor(typeof(Texture2DArray), false)]
	public class MixtureInspectorTexture2DArray : Editor
	{
		public override void OnInspectorGUI()
		{
			EditorGUILayout.LabelField("Hello World !");
		}

		public override void DrawPreview(Rect previewArea)
		{
			EditorGUI.DrawPreviewTexture(previewArea, target as Texture2D);
		}

		public override bool HasPreviewGUI() => true;

		public override void OnPreviewSettings()
		{
			GUILayout.Toggle(true, "Array slice");
		}

        // public override void OnPreviewGUI(Rect r, GUIStyle background)
		// {
		// 	// EditorGUI.DrawPreviewTexture(r, MixtureAssetCallbacks.Icon);
		// }
	}
}