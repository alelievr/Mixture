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
				if (oldTextureObject != null) // release memory and remove asset
				{
					AssetDatabase.RemoveObjectFromAsset(oldTextureObject);
					DestroyImmediate(oldTextureObject, true);
				}
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

			if (!(outputTexture is CustomRenderTexture))
			{
				outputTexture = new CustomRenderTexture(s.width, s.height, (GraphicsFormat)s.targetFormat);
			}

			var crt = outputTexture as CustomRenderTexture;
			bool needsUpdate = crt.width != s.width
				|| crt.height != s.height
				|| crt.useMipMap != outputNode.hasMips
				|| crt.volumeDepth != s.sliceCount
				|| crt.graphicsFormat != (GraphicsFormat)s.targetFormat
				|| crt.updateMode != CustomRenderTextureUpdateMode.Realtime;

			if (needsUpdate)
			{
				if (crt.IsCreated())
					crt.Release();
				crt.width = s.width;
				crt.height = s.height;
				crt.graphicsFormat = (GraphicsFormat)s.targetFormat;
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
					outputTexture = new Texture2D(s.width, s.height, (GraphicsFormat)s.targetFormat, creationFlags); // By default we compress the texture
					onOutputTextureUpdated?.Invoke();
					break;
				case OutputDimension.Texture3D:
					outputTexture = new Texture3D(s.width, s.height, s.sliceCount, (GraphicsFormat)s.targetFormat, creationFlags);
					onOutputTextureUpdated?.Invoke();
					break;
				case OutputDimension.CubeMap:
					outputTexture = new Cubemap(s.width, (GraphicsFormat)s.targetFormat, creationFlags);
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

                OutputDimension dimension = (OutputDimension)(external.rtSettings.dimension == OutputDimension.Default ? (OutputDimension)external.rtSettings.GetTextureDimension(this) : external.rtSettings.dimension);
                GraphicsFormat format = (GraphicsFormat)external.rtSettings.targetFormat;

                switch (dimension)
                {
                    case OutputDimension.Default:
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

        public void SaveMainTexture()
        {
            if (isRealtime)
                return;
            
            if (outputNode.rtSettings.width != outputTexture.width
                || outputNode.rtSettings.height != outputTexture.height)
                UpdateOutputTexture(false);

            ReadBackTexture(this.outputNode);
        }

        // Write the rendertexture value to the graph main texture asset, or to an external Texture
        protected void ReadBackTexture(OutputNode node, Texture externalTexture = null)
        {
            // Retrieve the texture from the GPU:
            var src = node.tempRenderTexture;
            int depth = src.dimension == TextureDimension.Cube ? 6 : src.volumeDepth;
            var request = AsyncGPUReadback.Request(src, 0, 0, src.width, 0, src.height, 0, depth, (r) => {
                WriteRequestResult(node, r, (externalTexture == null ? outputTexture : externalTexture));
            });

            request.Update();

            request.WaitForCompletion();
        }
        
        struct Color16
        {
            ushort  r;
            ushort  g;
            ushort  b;
        }

        protected void WriteRequestResult(OutputNode node, AsyncGPUReadbackRequest request, Texture output)
        {
            var outputFormat = node.rtSettings.GetGraphicsFormat(this);

            if (request.hasError)
            {
                Debug.LogError("Can't readback the texture from GPU");
                return;
            }

            void FetchSlice(int slice, Action<Color32[]> SetPixelsColor32, Action<Color[]> SetPixelsColor)
            {
                NativeArray<Color32> colors32;
                NativeArray<Color> colors;

                switch ((OutputFormat)outputFormat)
                {
                    case OutputFormat.RGBA_Float:
                        colors = request.GetData<Color>(slice);
                        SetPixelsColor(colors.ToArray());
                        break;
                    case OutputFormat.RGBA_LDR:
                    case OutputFormat.RGBA_sRGB:
                        colors32 = request.GetData<Color32>(slice);
                        SetPixelsColor32(colors32.ToArray());
                        break;
                    case OutputFormat.R8_Unsigned:
                        var r8Colors = request.GetData<byte>(slice);
                        SetPixelsColor32(r8Colors.Select(r => new Color32(r, 0, 0, 0)).ToArray());
                        break;
                    case OutputFormat.R16: // For now we don't support half readback
                    case OutputFormat.RGBA_Half: // For now we don't support half readback
                        // var r8Colors = request.GetData<short>(slice);
                        // SetPixelsColor32(r8Colors.Select(r => new Color32(r, 0, 0, 0)).ToArray());
                    default:
                        Debug.LogError("Can't readback an image with format: " + outputFormat);
                        break;
                }
            }

            switch (output)
            {
                case Texture2D t:
                    if (outputFormat != t.graphicsFormat || node.enableCompression)
                    {
                        // If the output texture is compressed, then we can't readback the data inside it directly
                        var tempTexture = new Texture2D(t.width, t.height, node.rtSettings.GetGraphicsFormat(this), TextureCreationFlags.None);
                        FetchSlice(0, tempTexture.SetPixels32, tempTexture.SetPixels);
                        tempTexture.Apply();

                        // Copy the readback texture into the compressed one (replace it)
                        EditorUtility.CopySerialized(tempTexture, t);
                        UnityEngine.Object.DestroyImmediate(tempTexture);

                        if (node.enableCompression && node == outputNode)
                            CompressTexture(t);

                        // Trick to re-generate the preview and update the texture when the asset was changed
                        AssetDatabase.ImportAsset(this.mainAssetPath);
                        AssetDatabase.SaveAssets();
                        AssetDatabase.ImportAsset(this.mainAssetPath);
                    }
                    else
                    {
                        FetchSlice(0, t.SetPixels32, t.SetPixels);
                        t.Apply();
                    }
                    break;
                case Texture2DArray t:
                    for (int i = 0; i < node.tempRenderTexture.volumeDepth; i++)
                        FetchSlice(i, colors => t.SetPixels32(colors, i), colors => t.SetPixels(colors, i));
                    t.Apply();
                    break;
                case Texture3D t:
                    List<Color32> colors32List = new List<Color32>();
                    List<Color> colorsList = new List<Color>();
                    for (int i = 0; i < node.tempRenderTexture.volumeDepth; i++)
                        FetchSlice(i, c => colors32List.AddRange(c), c => colorsList.AddRange(c));

                    if (colors32List.Count != 0)
                        t.SetPixels32(colors32List.ToArray());
                    else
                        t.SetPixels(colorsList.ToArray());

                    t.Apply();
                    break;
                case Cubemap t:
                    for (int i = 0; i < 6; i++)
                        FetchSlice(i, c => t.SetPixels(c.Cast<Color>().ToArray(), (CubemapFace)i, 0), c => t.SetPixels(c, (CubemapFace)i, 0));

                    t.Apply();
                    break;
                default:
                    Debug.LogError(output + " is not a supported type for saving");
                    return;
            }

            EditorGUIUtility.PingObject(output);
        }

        void CompressTexture(Texture2D t)
        {
#if UNITY_EDITOR
            EditorUtility.CompressTexture(t, (TextureFormat)outputNode.compressionFormat, (UnityEditor.TextureCompressionQuality)outputNode.compressionQuality);
#endif
        }

    }
}