using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEditor.ShaderGraph;
using UnityEngine;

namespace Mixture {
	public static class MixtureShaderGraphCallbacks
	{
			public static readonly string	customMipMapShaderTemplate = "Templates/CustomMipMapTemplate";

			[MenuItem("Assets/Create/Mixture/Fixed Shader Graph", false, 202)]
			[MenuItem("Assets/Create/Shader/Custom Texture Graph", false, 200)]
			public static void CreateCustomTextureShaderGraph()
			{
				var graphItem = ScriptableObject.CreateInstance< CustomtextureShaderGraphAction >();
				ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
					0,
					graphItem,
					$"New Custom Texture Graph.{ShaderGraphImporter.Extension}",
					Resources.Load<Texture2D>("sg_graph_icon@64"),
					null
				);
			}

			[MenuItem("Assets/Create/Mixture/Custom Mip Map Shader", false, 903)]
			public static void CreateCustomMipMapShaderGraph()
			{
				var template = Resources.Load< TextAsset >(customMipMapShaderTemplate);
				ProjectWindowUtil.CreateScriptAssetFromTemplateFile(AssetDatabase.GetAssetPath(template), "Custom MipMap");
			}

			static Shader ReimportShaderGraphResource(string resourcePath)
			{
				string fullPath = "Packages/com.alelievr.mixture/Editor/Resources/" + resourcePath;

				AssetDatabase.ImportAsset(fullPath, ImportAssetOptions.DontDownloadFromCacheServer | ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);

				return Resources.Load< Shader >(resourcePath);
			}

			class CustomtextureShaderGraphAction : EndNameEditAction
			{
				public static readonly string template = "Templates/CustomTextureGraphTemplate";

				public override void Action(int instanceId, string pathName, string resourceFile)
				{
					var s = Resources.Load(template, typeof(Shader));

					// In case there was a compilation error sg files can be broken so we re-import them
					if (s == null)
						s = ReimportShaderGraphResource(template);

					AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(s), pathName);
					ProjectWindowUtil.ShowCreatedAsset(AssetDatabase.LoadAssetAtPath<Shader>(pathName));
				}
			}
	}
}
