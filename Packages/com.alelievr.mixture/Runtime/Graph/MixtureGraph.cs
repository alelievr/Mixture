using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;
using System;
using Object = UnityEngine.Object;
using Unity.Collections;
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

        public bool             isParameterViewOpen;

        [System.NonSerialized]
		OutputNode		        _outputNode;
		public OutputNode		outputNode
        {
            get
            {
                if (_outputNode == null)
                    _outputNode = nodes.FirstOrDefault(n => n is OutputNode) as OutputNode;

                return _outputNode;
            }
            internal set => _outputNode = value;
        }

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
#if UNITY_EDITOR
				if (!String.IsNullOrEmpty(_mainAssetPath) && AssetDatabase.IsMainAssetAtPathLoaded(_mainAssetPath))
					return _mainAssetPath;
				else
					return _mainAssetPath = AssetDatabase.GetAssetPath(this);
#else
                return null;
#endif
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
			if (outputNode == null)
				outputNode = AddNode(BaseNode.CreateFromType< OutputNode >(Vector2.zero)) as OutputNode;

#if UNITY_EDITOR
            // TODO: check if the asset is in a Resources folder for realtime and put a warning if it's not the case
            // + store the Resources path in a string
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

        public bool                 IsObjectInGraph(Object obj) => objectReferences.Contains(obj);

		public void					RemoveObjectFromGraph(Object obj)
		{
            if (obj == null)
                return;

			objectReferences.Remove(obj);

#if UNITY_EDITOR
			AssetDatabase.RemoveObjectFromAsset(obj);
#endif
		}

        public void                 ClearObjectReferences() => objectReferences.Clear();

		/// <summary>
		/// Warning: this function will create a new output texture from scratch, It means that you will loose all data in the former outputTexture
		/// </summary>
		public void					UpdateOutputTexture(bool updateMainAsset = true)
		{
			Texture		oldTextureObject = outputTexture;

			if (isRealtime)
			{
				UpdateOutputRealtimeTexture();
				// We don't ever need to the main asset in realtime if it's already a CRT
				if (oldTextureObject is CustomRenderTexture)
					updateMainAsset = false;
#if UNITY_EDITOR
				else
					RealtimeMixtureReferences.realtimeMixtureCRTs.Add(outputTexture as CustomRenderTexture);
#endif
			}
			else
				UpdateOutputStaticTexture();

			// In editor we need to refresh the main asset view
#if UNITY_EDITOR
			if (updateMainAsset)
			{
                UpdateMainAsset(oldTextureObject);
			}
#endif
		}

#if UNITY_EDITOR
        void UpdateMainAsset(Texture oldTextureObject)
        {
            if (oldTextureObject == outputTexture)
                return;

            if (oldTextureObject != null) // release memory and remove asset
            {
                AssetDatabase.RemoveObjectFromAsset(oldTextureObject);
                DestroyImmediate(oldTextureObject, true);
            }

            AssetDatabase.AddObjectToAsset(outputTexture, this);
            AssetDatabase.SetMainObject(outputTexture, mainAssetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (Selection.activeObject == oldTextureObject)
                Selection.activeObject = outputTexture;
        }
#endif

		void UpdateOutputRealtimeTexture()
		{
			var s = outputNode.rtSettings;

			if (!(outputTexture is CustomRenderTexture))
			{
				outputTexture = new CustomRenderTexture(s.width, s.height, s.graphicsFormat) { name = "Realtime Final Copy", enableRandomWrite = true };
			}

			var crt = outputTexture as CustomRenderTexture;
			bool needsUpdate = crt.width != s.width
				|| crt.height != s.height
				|| crt.useMipMap != outputNode.hasMips
				|| crt.volumeDepth != s.sliceCount
				|| crt.graphicsFormat != (GraphicsFormat)s.graphicsFormat
				|| crt.updateMode != CustomRenderTextureUpdateMode.Realtime;

			if (needsUpdate)
			{
				if (crt.IsCreated())
					crt.Release();
				crt.width = s.width;
				crt.height = s.height;
				crt.graphicsFormat = (GraphicsFormat)s.graphicsFormat;
				crt.useMipMap = outputNode.hasMips;
				crt.autoGenerateMips = false;
				crt.updateMode = CustomRenderTextureUpdateMode.Realtime;
				crt.volumeDepth = s.sliceCount;
				crt.Create();
			}
		}

		void UpdateOutputStaticTexture()
		{
			var s = outputNode.rtSettings;
            var creationFlags = outputNode.hasMips ? TextureCreationFlags.MipChain : TextureCreationFlags.None;

			// TODO: compression options (TextureCreationFlags.Crunch)
			switch (outputNode.rtSettings.dimension)
			{
				case OutputDimension.Texture2D:
					outputTexture = new Texture2D(s.width, s.height, (GraphicsFormat)s.graphicsFormat, creationFlags); // By default we compress the texture
					onOutputTextureUpdated?.Invoke();
					break;
				case OutputDimension.Texture3D:
					outputTexture = new Texture3D(s.width, s.height, s.sliceCount, (GraphicsFormat)s.graphicsFormat, creationFlags);
					onOutputTextureUpdated?.Invoke();
					break;
				case OutputDimension.CubeMap:
					outputTexture = new Cubemap(s.width, (GraphicsFormat)s.graphicsFormat, creationFlags);
					onOutputTextureUpdated?.Invoke();
					break;
				default:
					Debug.LogError("Texture format " + s.dimension + " is not supported");
					return;
			}
		}

#if UNITY_EDITOR
        public void SaveExternalTexture(ExternalOutputNode external, bool saveAs = false)
        {
            try
            {
                Texture outputTexture = null;
                bool isHDR = external.rtSettings.isHDR;

                OutputDimension dimension = (OutputDimension)(external.rtSettings.dimension == OutputDimension.SameAsOutput ? (OutputDimension)external.rtSettings.GetTextureDimension(this) : external.rtSettings.dimension);
                GraphicsFormat format = (GraphicsFormat)external.rtSettings.graphicsFormat;

                switch (dimension)
                {
                    case OutputDimension.SameAsOutput:
                    case OutputDimension.Texture2D:
                        outputTexture = new Texture2D(external.tempRenderTexture.width, external.tempRenderTexture.height, format, TextureCreationFlags.MipChain);
                        break;
                    case OutputDimension.CubeMap:
                        outputTexture = new Cubemap(external.tempRenderTexture.width, format, TextureCreationFlags.MipChain);
                        break;
                    case OutputDimension.Texture3D:
                        outputTexture = new Texture3D(external.rtSettings.width, external.rtSettings.height, external.rtSettings.sliceCount, format, TextureCreationFlags.MipChain);
                        break;
                }
                EditorUtility.DisplayProgressBar("Mixture", "Reading Back Data...", 0.1f);
                ReadBackTexture(external, outputTexture);

                // Check Output Type
                string assetPath;
                if (external.asset != null && !saveAs)
                    assetPath = AssetDatabase.GetAssetPath(external.asset);
                else
                {
                    string extension = "asset";

                    if (dimension == OutputDimension.Texture2D)
                    {
                        if (isHDR)
                            extension = "exr";
                        else
                            extension = "png";
                    }

                    assetPath = EditorUtility.SaveFilePanelInProject("Save Texture", System.IO.Path.GetDirectoryName(AssetDatabase.GetAssetPath(this)) + "/ExternalTexture", extension, "Save Texture");

                    if (string.IsNullOrEmpty(assetPath))
                    {
                        EditorUtility.ClearProgressBar();
                        return; // Canceled
                    }
                }
                EditorUtility.DisplayProgressBar("Mixture", $"Writing to {assetPath}...", 0.3f);

                if (dimension == OutputDimension.Texture3D)
                {
                    var volume = AssetDatabase.LoadAssetAtPath<Texture3D>(assetPath);
                    if (volume == null)
                    {
                        volume = new Texture3D(external.rtSettings.width, external.rtSettings.height, external.rtSettings.sliceCount, format, TextureCreationFlags.MipChain);
                        AssetDatabase.CreateAsset(volume, assetPath);
                    }
                    volume.SetPixels((outputTexture as Texture3D).GetPixels());
                    volume.Apply();
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();

                    external.asset = volume;
                }
                else if (dimension == OutputDimension.Texture2D)
                {
                    byte[] contents = null;

                    if (isHDR)
                        contents = ImageConversion.EncodeToEXR(outputTexture as Texture2D);
                    else
                    {

                        var colors = (outputTexture as Texture2D).GetPixels();
                        for (int i = 0; i < colors.Length; i++)
                        {
                            colors[i] = colors[i].gamma;
                        }
                        (outputTexture as Texture2D).SetPixels(colors);

                        contents = ImageConversion.EncodeToPNG(outputTexture as Texture2D);
                    }

                    System.IO.File.WriteAllBytes(System.IO.Path.GetDirectoryName(Application.dataPath) + "/" + assetPath, contents);

                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();

                    TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(assetPath);
                    switch (external.external2DOoutputType)
                    {
                        case ExternalOutputNode.External2DOutputType.Color:
                            importer.textureType = TextureImporterType.Default;
                            importer.sRGBTexture = true;
                            break;
                        case ExternalOutputNode.External2DOutputType.Linear:
                            importer.textureType = TextureImporterType.Default;
                            importer.sRGBTexture = false;
                            break;
                        case ExternalOutputNode.External2DOutputType.Normal:
                            importer.textureType = TextureImporterType.NormalMap;
                            break;
                        case ExternalOutputNode.External2DOutputType.LatLonCubemap:
                            importer.textureShape = TextureImporterShape.TextureCube;
                            importer.generateCubemap = TextureImporterGenerateCubemap.Cylindrical;
                            break;
                    }
                    importer.SaveAndReimport();

                    if(external.external2DOoutputType == ExternalOutputNode.External2DOutputType.LatLonCubemap)
                        external.asset = AssetDatabase.LoadAssetAtPath<Cubemap>(assetPath);
                    else
                        external.asset = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);

                }
                else if (dimension == OutputDimension.CubeMap)
                {
                    throw new System.NotImplementedException(); // Todo : write as 2D Cubemap : Perform LatLon Conversion + Reimport
                                                                //System.IO.File.WriteAllBytes(assetPath, ImageConversion.EncodeToPNG(outputTexture as Cubemap).);
                }
                EditorUtility.DisplayProgressBar("Mixture", $"Importing {assetPath}...", 1.0f);

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }
#endif

        public void ReadbackMainTexture(Texture target)
        {
            if (isRealtime)
            {
                Debug.LogError("Can't save runtime texture to a specified path.");
                return;
            }

            ReadBackTexture(outputNode, target);
        }

#if UNITY_EDITOR
        public void SaveMainTexture()
        {
            if (isRealtime)
                return;
            
            // We only need to update the main asset texture because the outputTexture should
            // always be correctly setup when we arrive here.
            var currentTexture = AssetDatabase.LoadAssetAtPath<Texture>(mainAssetPath);
            UpdateMainAsset(currentTexture);

            ReadBackTexture(this.outputNode);

            EditorGUIUtility.PingObject(outputTexture);
        }
#endif

        public struct ReadbackData
        {
            public OutputNode   node;
            public Texture      targetTexture;
            public int          mipLevel;
        }

        // Write the rendertexture value to the graph main texture asset, or to an external Texture
        protected void ReadBackTexture(OutputNode node, Texture externalTexture = null)
        {
            var outputFormat = node.rtSettings.GetGraphicsFormat(this);
            var src = node.tempRenderTexture;
            var target = externalTexture == null ? outputTexture : externalTexture;

            // When we use Texture2D, we can compress them. In that case we use a temporary target for readback before compressing / converting it.
#if UNITY_EDITOR
            bool useTempTarget = node.enableCompression || outputFormat != target.graphicsFormat;
            useTempTarget &= node.rtSettings.GetTextureDimension(this) == TextureDimension.Tex2D;
#else
            // We can't compress the texture in real-time
            bool useTempTarget = false;
#endif

            if (useTempTarget)
            {
                var textureFlags = node.hasMips ? TextureCreationFlags.MipChain : TextureCreationFlags.MipChain;
                target = new Texture2D(target.width, target.height, outputFormat, textureFlags);
            }

            var readbackRequests = new List<AsyncGPUReadbackRequest>();
            for (int mipLevel = 0; mipLevel < node.tempRenderTexture.mipmapCount; mipLevel++)
            {
                int width = src.width / (1 << mipLevel);
                int height = src.height / (1 << mipLevel);
                int depth = src.dimension == TextureDimension.Cube ? 6 : (src.dimension == TextureDimension.Tex2D ? 1 : Mathf.Max(src.volumeDepth / (1 << mipLevel), 1));
                var data = new ReadbackData{
                    node = node,
                    targetTexture = target,
                    mipLevel = mipLevel
                };
                var request = AsyncGPUReadback.Request(src, mipLevel, 0, width, 0, height, 0, depth, (r) => {
                    WriteRequestResult(r, data);
                });

                request.Update();
                readbackRequests.Add(request);
            }

            foreach (var r in readbackRequests)
                r.WaitForCompletion();

            if (useTempTarget)
                CompressTexture(target, externalTexture == null ? outputTexture : externalTexture);
        }
        
        protected void WriteRequestResult(AsyncGPUReadbackRequest request, ReadbackData data)
        {
            var outputPrecision = data.node.rtSettings.GetOutputPrecision(this);
            var outputChannels = data.node.rtSettings.GetOutputChannels(this);

            if (request.hasError)
            {
                Debug.LogError("Can't readback the texture from GPU");
                return;
            }

            switch (data.targetTexture)
            {
                case Texture2D t:
                    t.SetPixelData(request.GetData<float>(0), data.mipLevel);
                    t.Apply(false);
                    break;
                case Texture3D t:
                    List<float> rawData = new List<float>();

                    int sliceCount = Mathf.Max(data.node.tempRenderTexture.volumeDepth / (1 << data.mipLevel), 1);
                    for (int i = 0; i < sliceCount; i++)
                        rawData.AddRange(request.GetData<float>(i).ToList());

                    t.SetPixelData(rawData.ToArray(), data.mipLevel);
                    t.Apply(false);
                    break;
                case Cubemap t:
                    for (int i = 0; i < 6; i++)
                        t.SetPixelData(request.GetData<float>(i), data.mipLevel, (CubemapFace)i);

                    t.Apply(false);
                    break;
                default:
                    Debug.LogError(data.targetTexture + " is not a supported type for saving");
                    return;
            }
        }

        /// <summary>
        /// This only works for Texture2D
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        void CompressTexture(Texture source, Texture destination)
        {
#if UNITY_EDITOR
            // Copy the readback texture into the compressed one (replace it)
            EditorUtility.CopySerialized(source, destination);
            UnityEngine.Object.DestroyImmediate(source);

            EditorUtility.CompressTexture(destination as Texture2D, (TextureFormat)outputNode.compressionFormat, (UnityEditor.TextureCompressionQuality)outputNode.compressionQuality);

            // Trick to re-generate the preview and update the texture when the asset was changed
            AssetDatabase.ImportAsset(this.mainAssetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(this.mainAssetPath);
#endif
        }
    }
}