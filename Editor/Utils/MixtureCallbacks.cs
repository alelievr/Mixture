using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.Linq;
using UnityEditor.ProjectWindowCallback;
using System.IO;
using UnityEditor.ShaderGraph;
using System.Reflection;

namespace Mixture
{
	public class MixtureAssetCallbacks
	{
		public static readonly string	Extension = "asset";
		public static readonly string	customTextureShaderTemplate = "Templates/CustomTextureShaderTemplate";

		public static readonly string	mixtureShaderNodeCSharpTemplate = "Templates/MixtureShaderNodeTemplate";
		public static readonly string	mixtureShaderNodeDefaultName = "MixtureShaderNode.cs";

		[MenuItem("Assets/Create/Mixture/Mixture Graph", false, 100)]
		public static void CreateMixtureGraph()
		{
			var graphItem = ScriptableObject.CreateInstance< MixtureGraphAction >();
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, graphItem,
                $"New Mixture Graph.{Extension}", MixtureUtils.icon, null);
		}
		
		[MenuItem("Assets/Create/Mixture/C# Shader Node", false, 101)]
		public static void CreateMixtureNode()
		{
			var template = Resources.Load< TextAsset >(mixtureShaderNodeCSharpTemplate);
			ProjectWindowUtil.CreateScriptAssetFromTemplateFile(AssetDatabase.GetAssetPath(template), mixtureShaderNodeDefaultName);
		}

		[MenuItem("Assets/Create/Shader/Custom Texture", false, 100)]
		public static void CreateCustomTextureShader()
		{
			var graphItem = ScriptableObject.CreateInstance< CustomTextureShaderAction >();
			var shaderTemplate = Resources.Load(customTextureShaderTemplate, typeof(Shader));
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, graphItem,
                $"New Custom Texture Shader.shader",
				EditorGUIUtility.ObjectContent(null, typeof(Shader)).image as Texture2D,
				AssetDatabase.GetAssetPath(shaderTemplate)
			);
		}

		[MenuItem("Assets/Create/Shader/Custom Texture Graph", false, 200)]
		public static void CreateCustomTextureShaderGraph()
		{
			var graphItem = ScriptableObject.CreateInstance< CustomtextureShaderGraphAction >();
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, graphItem,
                $"New Custom Texture Graph.{ShaderGraphImporter.Extension}", Resources.Load<Texture2D>("sg_graph_icon@64"), null);
		}

		[OnOpenAsset(0)]
		public static bool OnBaseGraphOpened(int instanceID, int line)
		{
			var asset = EditorUtility.InstanceIDToObject(instanceID);

			if (asset is Texture)
			{
				// Check if the CustomRenderTexture we're opening is a Mixture graph
				var path = AssetDatabase.GetAssetPath(EditorUtility.InstanceIDToObject(instanceID));
				var graph = AssetDatabase.LoadAllAssetsAtPath(path).FirstOrDefault(o => o is MixtureGraph) as MixtureGraph;

				if (graph == null)
					return false;

				MixtureGraphWindow.Open().InitializeGraph(graph);
				return true;
			}
			return false;
		}

		class MixtureGraphAction : EndNameEditAction
		{
			public override void Action(int instanceId, string pathName, string resourceFile)
			{
				var mixture = ScriptableObject.CreateInstance< MixtureGraph >();
				mixture.name = Path.GetFileNameWithoutExtension(pathName);
				mixture.hideFlags = HideFlags.HideInHierarchy;

				AssetDatabase.CreateAsset(mixture, pathName);

				// Generate the output texture:
				mixture.UpdateOutputTexture(false);

				// Then set it as main object
				AssetDatabase.AddObjectToAsset(mixture.outputTexture, mixture);
				AssetDatabase.SetMainObject(mixture.outputTexture, pathName);
				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();

				EditorGUIUtility.PingObject(mixture.outputTexture);
			}
		}

		class CustomTextureShaderAction : EndNameEditAction
		{
			static MethodInfo	createScriptAsset = typeof(ProjectWindowUtil).GetMethod("CreateScriptAssetFromTemplate", BindingFlags.Static | BindingFlags.NonPublic);

			public override void Action(int instanceId, string pathName, string resourceFile)
			{
				if (!File.Exists(resourceFile))
                {
                    Debug.LogError("Can't find template: " + resourceFile);
                    return ;
                }

				createScriptAsset.Invoke(null, new object[]{ pathName, resourceFile });
				ProjectWindowUtil.ShowCreatedAsset(AssetDatabase.LoadAssetAtPath<Shader>(pathName));
				AssetDatabase.Refresh();
			}
		}

		class CustomtextureShaderGraphAction : EndNameEditAction
		{
			public static readonly string template = "Templates/CustomTextureGraphTemplate";

			public override void Action(int instanceId, string pathName, string resourceFile)
			{
				var s = Resources.Load(template, typeof(Shader));
				AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(s), pathName);
				ProjectWindowUtil.ShowCreatedAsset(AssetDatabase.LoadAssetAtPath<Shader>(pathName));
			}
		}
	}
}