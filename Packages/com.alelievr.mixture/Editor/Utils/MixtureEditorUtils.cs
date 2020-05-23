using UnityEngine;
using UnityEditor;
using System.IO;

namespace Mixture
{
    public static class MixtureEditorUtils
    {
        public static readonly string   mixtureEditorResourcesPath = "Packages/com.alelievr.mixture/Editor/Resources/";
        public static readonly string   shaderGraphTexture2DTemplate = "Templates/ShaderGraphTexture2DTemplate";
        public static readonly string   shaderGraphTexture3DTemplate = "Templates/ShaderGraphTexture3DTemplate";
        public static readonly string   shaderGraphTextureCubeTemplate = "Templates/ShaderGraphTextureCubeTemplate";
        public static readonly string   shaderTextTexture2DTemplate = "Templates/ShaderTextTexture2DTemplate";
        public static readonly string   shaderTextTexture3DTemplate = "Templates/ShaderTextTexture3DTemplate";
        public static readonly string   shaderTextTextureCubeTemplate = "Templates/ShaderTextTextureCubeTemplate";
        public static readonly string   computeShaderTemplate = "Templates/ComputeTemplate";

        static string GetCurrentProjectWindowPath()
        {
            string path = "Assets";

            foreach (UnityEngine.Object obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets))
            {
                path = AssetDatabase.GetAssetPath(obj);
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    path = Path.GetDirectoryName(path);
                    break;
                }
            }
            
            return path;
		}

        static string       GetAssetTemplatePath< T >(string name) where T : Object
            => AssetDatabase.GetAssetPath(Resources.Load< T >(name));

        static T            CopyAssetWithNameFromTemplate<T>(string name, string templatePath) where T : Object
        {
            var newPath = GetCurrentProjectWindowPath() + "/" + name;
            newPath = AssetDatabase.GenerateUniqueAssetPath(newPath);

            AssetDatabase.CopyAsset(templatePath, newPath);

            AssetDatabase.Refresh();

            var asset = AssetDatabase.LoadAssetAtPath< T >(newPath);
            EditorGUIUtility.PingObject(asset);
            return asset;
        }

		public static Shader    CreateNewShaderGraph(string name, OutputDimension dimension)
		{
            name += ".shadergraph";
			switch (dimension)
			{
				case OutputDimension.Texture2D:
                    return CopyAssetWithNameFromTemplate<Shader>(name, GetAssetTemplatePath<Shader>(shaderGraphTexture2DTemplate));
				case OutputDimension.Texture3D:
                    return CopyAssetWithNameFromTemplate<Shader>(name, GetAssetTemplatePath<Shader>(shaderGraphTexture3DTemplate));
				case OutputDimension.CubeMap:
                    return CopyAssetWithNameFromTemplate<Shader>(name, GetAssetTemplatePath<Shader>(shaderGraphTextureCubeTemplate));
                default:
                    Debug.LogError("Can't find template to create new shader for dimension: " + dimension);
                    return null;
			}
		}

		public static Shader    CreateNewShaderText(string name, OutputDimension dimension)
		{
            name += ".shader";
			switch (dimension)
			{
				case OutputDimension.Texture2D:
                    return CopyAssetWithNameFromTemplate<Shader>(name, GetAssetTemplatePath<Shader>(shaderTextTexture2DTemplate));
				case OutputDimension.Texture3D:
                    return CopyAssetWithNameFromTemplate<Shader>(name, GetAssetTemplatePath<Shader>(shaderTextTexture3DTemplate));
				case OutputDimension.CubeMap:
                    return CopyAssetWithNameFromTemplate<Shader>(name, GetAssetTemplatePath<Shader>(shaderTextTextureCubeTemplate));
                default:
                    Debug.LogError("Can't find template to create new shader for dimension: " + dimension);
                    return null;
			}
		}

        public static ComputeShader CreateEmbeddedComputeShader(string mainObjectPath, string name)
        {
            name += ".compute";
            var path = GetAssetTemplatePath<ComputeShader>(computeShaderTemplate);
            var cs = AssetDatabase.LoadAssetAtPath<ComputeShader>(path);

            AssetDatabase.AddObjectToAsset(cs, mainObjectPath);
            return cs;
        }

        public static ComputeShader CreateComputeShader(string name)
        {
            name += ".compute";
            return CopyAssetWithNameFromTemplate<ComputeShader>(name, GetAssetTemplatePath<ComputeShader>(computeShaderTemplate));
        }

		public static void ToggleMode(MixtureGraph mixture)
		{
			mixture.isRealtime = !mixture.isRealtime;
            AssetDatabase.SaveAssets();
            mixture.UpdateOutputTexture(true);
			AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(mixture), ImportAssetOptions.ForceUpdate | ImportAssetOptions.ImportRecursive | ImportAssetOptions.DontDownloadFromCacheServer);
            AssetDatabase.Refresh();
            MixtureGraphWindow.Open(mixture);
		}
    }
}