using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;
using System;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Mixture
{
	[System.Serializable]
	public class MixtureGraph : BaseGraph
	{
		// Serialized data for the editor:
		public bool				realtimePreview;

		// Whether or not the mixture is realtime
		public bool				isRealtime;

		public OutputNode		outputNode;

		[SerializeField]
		List< Object >			objectReferences = new List< Object >();

		[SerializeField]
		Texture					_outputTexture;
		public Texture			outputTexture
		{
			get
			{
#if UNITY_EDITOR
				if (_outputTexture == null)
					_outputTexture = AssetDatabase.LoadAssetAtPath< Texture >(mainAssetPath);
#endif
				return _outputTexture;
			}
			set => _outputTexture = value;
		}

		[System.NonSerialized]
		string					_mainAssetPath;
		public string			mainAssetPath
		{
			get
			{
				if (!String.IsNullOrEmpty(_mainAssetPath) && AssetDatabase.IsMainAssetAtPathLoaded(_mainAssetPath))
					return _mainAssetPath;
				else
					return _mainAssetPath = AssetDatabase.GetAssetPath(this);
			}
		}

		public event Action		onOutputTextureUpdated;

		public MixtureGraph()
		{
			base.onEnabled += Enabled;
		}

		void Enabled()
		{
			// We should have only one OutputNode per graph
			outputNode = nodes.FirstOrDefault(n => n is OutputNode) as OutputNode;

			if (outputNode == null)
				outputNode = AddNode(BaseNode.CreateFromType< OutputNode >(Vector2.zero)) as OutputNode;

#if UNITY_EDITOR
			if (isRealtime)
				RealtimeMixtureReferences.realtimeMixtureCRTs.Add(outputTexture as CustomRenderTexture);
#endif
		}

		public List< Object >		GetObjectsReferences()
		{
			return objectReferences;
		}

		public void					AddObjectToGraph(Object obj)
		{
			objectReferences.Add(obj);

#if UNITY_EDITOR
			AssetDatabase.AddObjectToAsset(obj, mainAssetPath);
#endif
		}

		public void					RemoveObjectFromGraph(Object obj)
		{
			objectReferences.Remove(obj);

#if UNITY_EDITOR
			AssetDatabase.RemoveObjectFromAsset(obj);
#endif
		}

		/// <summary>
		/// Warning: this function will create a new output texture from scratch, It means that you will loose all data in the former outputTexture
		/// </summary>
		public void					UpdateOutputTexture(bool updateMainAsset = true)
		{
			Texture		oldTextureObject = outputTexture;

			if (isRealtime)
			{
				UpdateOutputRealtimeTexture();
				// We don't ever need to change the main asset in realtime, it's always a CRT
				updateMainAsset = false;
			}
			else
				UpdateOutputStaticTexture();

			// In editor we need to refresh the main asset view
#if UNITY_EDITOR
			if (updateMainAsset)
			{
				if (oldTextureObject != null)
					AssetDatabase.RemoveObjectFromAsset(oldTextureObject);
				AssetDatabase.AddObjectToAsset(outputTexture, this);
				AssetDatabase.SetMainObject(outputTexture, mainAssetPath);
				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
			}
#endif
		}

		void UpdateOutputRealtimeTexture()
		{
			var s = outputNode.rtSettings;
			bool useMipMap = outputNode.mipmapCount > 1;

			if (!(outputTexture is CustomRenderTexture))
			{
				outputTexture = new CustomRenderTexture(s.width, s.height, (GraphicsFormat)s.targetFormat);
			}

			var crt = outputTexture as CustomRenderTexture;
			bool needsUpdate = crt.width != s.width
				|| crt.height != s.height
				|| crt.useMipMap != useMipMap
				|| crt.volumeDepth != s.sliceCount
				|| crt.graphicsFormat != (GraphicsFormat)s.targetFormat;

			if (needsUpdate)
			{
				if (crt.IsCreated())
					crt.Release();
				crt.width = s.width;
				crt.height = s.height;
				crt.graphicsFormat = (GraphicsFormat)s.targetFormat;
				crt.useMipMap = useMipMap;
				crt.autoGenerateMips = false;
				crt.updateMode = CustomRenderTextureUpdateMode.Realtime;
				crt.volumeDepth = s.sliceCount;
				crt.Create();
			}
		}

		void UpdateOutputStaticTexture()
		{
			var s = outputNode.rtSettings;

			// TODO: compression options (TextureCreationFlags.Crunch)
			switch (outputNode.rtSettings.dimension)
			{
				case OutputDimension.Texture2D:
					outputTexture = new Texture2D(s.width, s.height, (GraphicsFormat)s.targetFormat, outputNode.mipmapCount, TextureCreationFlags.None); // By default we compress the texture
					onOutputTextureUpdated?.Invoke();
					break;
				case OutputDimension.Texture3D:
					outputTexture = new Texture3D(s.width, s.height, s.sliceCount, (GraphicsFormat)s.targetFormat, TextureCreationFlags.None, outputNode.mipmapCount);
					onOutputTextureUpdated?.Invoke();
					break;
				case OutputDimension.CubeMap:
					outputTexture = new Cubemap(s.width, (GraphicsFormat)s.targetFormat, TextureCreationFlags.None, outputNode.mipmapCount);
					onOutputTextureUpdated?.Invoke();
					break;
				default:
					Debug.LogError("Texture format " + s.dimension + " is not supported");
					return;
			}
		}
	}
}