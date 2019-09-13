using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using GraphProcessor;
using System.Collections.Generic;
using UnityEngine.Rendering;
using Unity.Collections;
using System;
using System.Linq;
using TextureCompressionQuality = UnityEngine.TextureCompressionQuality;
using UnityEngine.Experimental.Rendering;

namespace Mixture
{
    [NodeCustomEditor(typeof(ExternalOutputNode))]
    public class ExternalOutputNodeView : OutputNodeView
    {

        protected override void SaveMasterTexture()
        {
            ExternalOutputNode external = outputNode as ExternalOutputNode;
            Texture outputTexture = null;
            bool isHDR = external.rtSettings.isHDR;

            OutputDimension dimension = (OutputDimension)(external.rtSettings.dimension == OutputDimension.Default ? (OutputDimension)external.rtSettings.GetTextureDimension(graph) : external.rtSettings.dimension);
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

            ReadBackTexture(outputTexture);

            // Check Output Type
            string assetPath;
            if (external.asset != null)
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

                assetPath = EditorUtility.SaveFilePanelInProject("Save Texture", AssetDatabase.GetAssetPath(graph) + "/ExternalTexture", extension, "Save Texture");

                if (string.IsNullOrEmpty(assetPath))
                    return; // Canceled
            }

            if(dimension == OutputDimension.Texture3D)
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
                    for(int i = 0; i < colors.Length; i++)
                    {
                        colors[i] = colors[i].gamma;
                    }
                    (outputTexture as Texture2D).SetPixels(colors);

                    contents = ImageConversion.EncodeToPNG(outputTexture as Texture2D);
                }

                System.IO.File.WriteAllBytes(System.IO.Path.GetDirectoryName(Application.dataPath) +"/"+assetPath, contents);

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
                external.asset = texture;
            }
            else if (dimension == OutputDimension.CubeMap)
            {
                throw new System.NotImplementedException(); // Todo : find a solution
                //System.IO.File.WriteAllBytes(assetPath, ImageConversion.EncodeToPNG(outputTexture as Cubemap).);
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

        }
    }
}