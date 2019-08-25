﻿using UnityEngine;
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

		public static readonly string	mixtureShaderNodeCSharpTemplate = "Templates/FixedShaderNodeTemplate";
		public static readonly string	mixtureShaderNodeCGTemplate = "Templates/FixedShaderTemplate";
		public static readonly string	mixtureShaderNodeDefaultName = "MixtureShaderNode.cs";
		public static readonly string	mixtureShaderName = "MixtureShader.shader";

		[MenuItem("Assets/Create/Mixture/Static Mixture Graph", false, 100)]
		public static void CreateStaticMixtureGraph()
		{
			var graphItem = ScriptableObject.CreateInstance< StaticMixtureGraphAction >();
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, graphItem,
                $"New Static Mixture Graph.{Extension}", MixtureUtils.icon, null);
		}

		[MenuItem("Assets/Create/Mixture/Realtime Mixture Graph", false, 101)]
		public static void CreateRealtimeMixtureGraph()
		{
			var graphItem = ScriptableObject.CreateInstance< RealtimeMixtureGraphAction >();
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, graphItem,
                $"New Realtime Mixture Graph.{Extension}", MixtureUtils.realtimeIcon, null);
		}
		
		[MenuItem("Assets/Create/Mixture/C# Fixed Shader Node", false, 200)]
		public static void CreateCSharpFixedShaderNode()
		{
			var template = Resources.Load< TextAsset >(mixtureShaderNodeCSharpTemplate);
			ProjectWindowUtil.CreateScriptAssetFromTemplateFile(AssetDatabase.GetAssetPath(template), mixtureShaderNodeDefaultName);
		}

		[MenuItem("Assets/Create/Mixture/Fixed Shader", false, 201)]
		public static void CreateCGFixedShaderNode()
		{
			var template = Resources.Load< TextAsset >(mixtureShaderNodeCGTemplate);
			ProjectWindowUtil.CreateScriptAssetFromTemplateFile(AssetDatabase.GetAssetPath(template), mixtureShaderName);
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

		[MenuItem("Assets/Create/Mixture/Fixed Shader Graph", false, 202)]
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

		abstract class MixtureGraphAction : EndNameEditAction
		{
			public abstract MixtureGraph CreateMixtureGraphAsset();

			public override void Action(int instanceId, string pathName, string resourceFile)
			{
				var mixture = CreateMixtureGraphAsset();
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

				ProjectWindowUtil.ShowCreatedAsset(mixture);
			}
		}

		class StaticMixtureGraphAction : MixtureGraphAction
		{
			// By default isRealtime is false so we don't need to initialize it like in the realtime mixture create function
			public override MixtureGraph CreateMixtureGraphAsset()
				=> ScriptableObject.CreateInstance< MixtureGraph >();
		}

		class RealtimeMixtureGraphAction : MixtureGraphAction
		{
			static MixtureGraph	_realtimeGraph;
			static MixtureGraph realtimeGraph
			{
				get
				{
					if (_realtimeGraph == null)
					{
						_realtimeGraph = ScriptableObject.CreateInstance< MixtureGraph >();
						_realtimeGraph.isRealtime = true;
						_realtimeGraph.realtimePreview = true;
					}
					return _realtimeGraph;
				}
			}

			public override MixtureGraph CreateMixtureGraphAsset()
			// We use Instantiate instead of CreateObject because we can use anther mixture graph that
			// have the parameters setup for realtime mixtures. This avoid the issue to have the ScriptableObject
			// OnEnable() function called before the isRealtime is assigned
				=> Object.Instantiate(realtimeGraph);
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