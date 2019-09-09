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

            Color p = (outputTexture as Texture2D).GetPixel(10, 10);
            Debug.Log(p);
            // Check Output Type

            string assetPath;
            if (external.asset != null)
                assetPath = AssetDatabase.GetAssetPath(external.asset);
            else
            {
                assetPath = System.IO.Path.GetDirectoryName(AssetDatabase.GetAssetPath(graph)) + "/" + external.assetName;
                if (dimension == OutputDimension.Texture3D)
                    assetPath += ".asset";
                else
                {
                    if (((OutputFormat)format).ToString().Contains("LDR"))
                        assetPath += ".png";
                    else
                        assetPath += ".exr";
                }
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
            }
            else if (dimension == OutputDimension.Texture2D)
            {
                byte[] contents = null;

                if (((OutputFormat)format).ToString().Contains("LDR"))
                    contents = ImageConversion.EncodeToPNG(outputTexture as Texture2D);
                else
                    contents = ImageConversion.EncodeToEXR(outputTexture as Texture2D);

                System.IO.File.WriteAllBytes(System.IO.Path.GetDirectoryName(Application.dataPath) +"/"+assetPath, contents);
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