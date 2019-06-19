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
		// Serialized datas for the editor:
		public bool				realtimePreview;

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
				if (!String.IsNullOrEmpty(_mainAssetPath))
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
		/// Warning: this function will create a new output texture from scratch, It means that you will loose all datas in the former outputTexture
		/// </summary>
		public void					UpdateOutputTexture(bool updateMainAsset = true)
		{
			Texture		oldTextureObject = outputTexture;

			// TODO: compression options (TextureCreationFlags.Crunch)
			switch (outputNode.dimension)
			{
				case TextureDimension.Tex2D:
					outputTexture = new Texture2D(outputNode.targetSize.x, outputNode.targetSize.y, outputNode.format, outputNode.mipmapCount, TextureCreationFlags.None); // By default we compress the texture
					onOutputTextureUpdated?.Invoke();
					break;
				case TextureDimension.Tex2DArray:
					outputTexture = new Texture2DArray(outputNode.targetSize.x, outputNode.targetSize.y, outputNode.sliceCount, outputNode.format, TextureCreationFlags.None, outputNode.mipmapCount);
					onOutputTextureUpdated?.Invoke();
					break;
				default:
					Debug.LogError("Texture format " + outputNode.dimension + " is not supported");
					return;
			}

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
	}
}