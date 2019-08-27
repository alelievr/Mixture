using UnityEngine;
using UnityEditor;
using System.IO;

namespace Mixture
{
    public static class MixtureEditorUtils
    {
        public static readonly string   shaderGraphTexture2DTemplate = "Templates/ShaderGraphTexture2DTemplate";
        public static readonly string   shaderGraphTexture3DTemplate = "Templates/ShaderGraphTexture3DTemplate";
        public static readonly string   shaderGraphTextureCubeTemplate = "Templates/ShaderGraphTextureCubeTemplate";
        public static readonly string   shaderTextTexture2DTemplate = "Templates/ShaderTextTexture2DTemplate";
        public static readonly string   shaderTextTexture3DTemplate = "Templates/ShaderTextTexture3DTemplate";
        public static readonly string   shaderTextTextureCubeTemplate = "Templates/ShaderTextTextureCubeTemplate";

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

        static string       GetShaderTemplatePath(string name)
        {
            var shader = Resources.Load< Shader >(name);
            return AssetDatabase.GetAssetPath(shader);
        }

        static Shader       CopyShaderWithNameFromTemplate(string name, string template)
        {
            var templatePath = GetShaderTemplatePath(template);
            var newPath = GetCurrentProjectWindowPath() + "/" + name;
            newPath = AssetDatabase.GenerateUniqueAssetPath(newPath);

            AssetDatabase.CopyAsset(templatePath, newPath);

            AssetDatabase.Refresh();

            var shader = AssetDatabase.LoadAssetAtPath< Shader >(newPath);
            EditorGUIUtility.PingObject(shader);
            return shader;
        }

		public static Shader    CreateNewShaderGraph(string name, OutputDimension dimension)
		{
            name += ".shadergraph";
			switch (dimension)
			{
				case OutputDimension.Texture2D:
                    return CopyShaderWithNameFromTemplate(name, shaderGraphTexture2DTemplate);
				case OutputDimension.Texture3D:
                    return CopyShaderWithNameFromTemplate(name, shaderGraphTexture3DTemplate);
				case OutputDimension.CubeMap:
                    return CopyShaderWithNameFromTemplate(name, shaderGraphTextureCubeTemplate);
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
                    return CopyShaderWithNameFromTemplate(name, shaderTextTexture2DTemplate);
				case OutputDimension.Texture3D:
                    return CopyShaderWithNameFromTemplate(name, shaderTextTexture3DTemplate);
				case OutputDimension.CubeMap:
                    return CopyShaderWithNameFromTemplate(name, shaderTextTextureCubeTemplate);
                default:
                    Debug.LogError("Can't find template to create new shader for dimension: " + dimension);
                    return null;
			}
		}

		public static void ToggleMode(MixtureGraph mixture)
		{
			mixture.isRealtime = !mixture.isRealtime;
            AssetDatabase.SaveAssets();
            mixture.UpdateOutputTexture(true);
			AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(mixture), ImportAssetOptions.ForceUpdate | ImportAssetOptions.ImportRecursive | ImportAssetOptions.DontDownloadFromCacheServer);
            AssetDatabase.Refresh();
            MixtureGraphWindow.Open().InitializeGraph(mixture);
		}
    }
}